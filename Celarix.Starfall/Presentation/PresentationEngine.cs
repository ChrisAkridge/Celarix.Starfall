using Celarix.Starfall.Layout;
using Celarix.Starfall.Presentation.Graph;
using Celarix.Starfall.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentation
{
    public class PresentationEngine<TScene, TTransition> : INotifyFrameRequested
         where TScene : class
         where TTransition : ITransition
    {
        private readonly PresentationEngineOptions options;
        private ILayoutEngine<TScene, TTransition> _layoutEngine;
        private readonly Dictionary<string, SceneNode<TScene>> nodes;
        private readonly List<TransitionEdge<TScene, TTransition>> edges;
        private string currentSceneId;
        private ActiveTransition<TScene, TTransition>? activeTransition;

        public string CurrentSceneId => currentSceneId;

        public PresentationEngine(PresentationEngineOptions options, ILayoutEngine<TScene, TTransition> layoutEngine)
        {
            this.options = options;
            _layoutEngine = layoutEngine;
            nodes = new Dictionary<string, SceneNode<TScene>>();
            edges = new List<TransitionEdge<TScene, TTransition>>();
        }

        public void OnFrameRequested(double deltaTime)
        {
            if (activeTransition != null)
            {
                var isComplete = activeTransition.Update(deltaTime);
                activeTransition.Render();
                if (isComplete)
                {
                    // ARCHITECTURE: This is where we finish switching to a new node IF a transition was defined for it.
                    currentSceneId = activeTransition.ToSceneId;
                    activeTransition = null;
                }

                // Don't render the node again until the next render cycle.
                return;
            }

            var currentNode = nodes.GetValueOrDefault(currentSceneId);
            if (currentNode != null)
            {
                // Update the layout engine with the current scene
                _layoutEngine.Render(currentNode.Scene);
            }
        }

        public void SetCurrentScene(string sceneId)
        {
            if (currentSceneId == sceneId)
            {
                if (options.ErrorLevel == ErrorLevel.Exception)
                {
                    throw new InvalidOperationException($"Scene '{sceneId}' is already the currently active scene.");
                }
                // TODO: Display error on Display level
                else { return; }
            }

            if (!nodes.ContainsKey(sceneId))
            {
                if (options.ErrorLevel == ErrorLevel.Exception)
                {
                    throw new InvalidOperationException($"Scene '{sceneId}' does not exist in the presentation graph.");
                }
                // TODO: Display error on Display level
                else { return; }
            }

            if (currentSceneId == null)
            {
                // This happens when we're starting the presentation. There's no transition to perform.
                // If you do want to i.e. fade from black, set up a black slide as the first scene and
                // transition from it to your actual first scene.
                currentSceneId = sceneId;
                return;
            }

            var fromScene = nodes[currentSceneId];
            var toScene = nodes[sceneId];
            var transitionEdge = edges.Find(e => e.From == fromScene && e.To == toScene);
            if (transitionEdge == null)
            {
                // Perform the "zero transition", which is just an instant switch to the new scene without any animation.
                currentSceneId = sceneId;
            }
            else
            {
                activeTransition = new ActiveTransition<TScene, TTransition>(transitionEdge, _layoutEngine, fromScene.Scene, toScene.Scene, sceneId);
            }
        }

        public void Start()
        {
            if (nodes.Count == 0 || !nodes.ContainsKey(currentSceneId))
            {
                // Always throw since we have nothing to draw on.
                throw new InvalidOperationException($"Cannot start presentation engine. There are no scenes or the current scene ID '{currentSceneId}' does not exist in the presentation graph.");
            }

            _layoutEngine.Start();
        }

        public void AddScene(string sceneId, TScene scene)
        {
            nodes[sceneId] = new SceneNode<TScene>(scene);
        }

        public void RemoveScene(string sceneId)
        {
            if (currentSceneId == sceneId)
            {
                if (options.ErrorLevel == ErrorLevel.Exception)
                {
                    throw new InvalidOperationException($"Cannot remove the currently active scene '{sceneId}'.");
                }
                // TODO: Render error on Display level
                return;
            }

            var matchingNode = nodes.GetValueOrDefault(sceneId);
            if (matchingNode != null)
            {
                nodes.Remove(sceneId);
                edges.RemoveAll(e => e.From == matchingNode || e.To == matchingNode);
            }
        }

        public void AddTransition(string fromSceneId, string toSceneId, TTransition transition)
        {
            var fromNode = nodes.GetValueOrDefault(fromSceneId);
            var toNode = nodes.GetValueOrDefault(toSceneId);

            if (ManualTransitionExists(fromSceneId, toSceneId))
            {
                if (options.ErrorLevel == ErrorLevel.Exception)
                {
                    throw new InvalidOperationException($"Cannot add new transition, a transition from '{fromSceneId}' to '{toSceneId}' already exists.");
                }
                // TODO: Render error on Display level
                return;
            }

            if (fromNode != null && toNode != null)
            {
                edges.Add(new TransitionEdge<TScene, TTransition>(fromNode, toNode, transition));
            }
        }

        public bool ManualTransitionExists(string fromSceneId, string toSceneId)
        {
            var fromNode = nodes.GetValueOrDefault(fromSceneId);
            var toNode = nodes.GetValueOrDefault(toSceneId);
            if (fromNode != null && toNode != null)
            {
                return edges.Exists(e => e.From == fromNode && e.To == toNode);
            }
            return false;
        }
    }
}
