using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Graph;

public sealed class ForceFactory
{
    public RepulsionForce BuildRepulsion(bool adjustBySize, double coefficient)
    {
        if (adjustBySize)
        {
            return new LinearRepulsionAntiCollision(coefficient);
        }
        else
        {
            return new LinearRepulsion(coefficient);
        }
    }

    public AttractionForce BuildAttraction(bool logAttraction,
        bool distributedAttraction,
        bool adjustBySize,
        double coefficient)
    {
        if (adjustBySize)
        {
            if (logAttraction)
            {
                if (distributedAttraction)
                {
                    return new LogAttractionDegreeDistributedAntiCollision(coefficient);
                }
                else
                {
                    return new LogAttractionAntiCollision(coefficient);
                }
            }
            else
            {
                if (distributedAttraction)
                {
                    return new LinearAttractionDegreeDistributedAntiCollision(coefficient);

                }
                else
                {
                    return new LinearAttractionAntiCollision(coefficient);
                }
            }
        }
        else
        {
            if (logAttraction)
            {
                if (distributedAttraction)
                {
                    return new LogAttractionDegreeDistributed(coefficient);
                }
                else
                {
                    return new LogAttraction(coefficient);
                }
            }
            else
            {
                if (distributedAttraction)
                {
                    return new LinearAttractionMassDistributed(coefficient);
                }
                else
                {
                    return new LinearAttraction(coefficient);
                }
            }
        }
    }
}
