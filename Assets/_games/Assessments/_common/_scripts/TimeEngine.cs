﻿using UnityEngine;
using System.Collections.Generic;

namespace EA4S.Assessment
{
    public class TimeEngine : ITickable
    {
        static TimeEngine instance;
        public static TimeEngine Instance
        {
            get
            {
                if (instance == null)
                    instance = new TimeEngine();
                return instance;
            }
        }

        List<ITickable> yieldInstructions = new List< ITickable>();

        public void Clear()
        {
            yieldInstructions.Clear();
        }

        public CustomYieldInstruction WaitForSeconds( float seconds)
        {
            var wait = new PausableWait( seconds);
            yieldInstructions.Add( wait);
            return wait;
        }

        public static CustomYieldInstruction Wait(float seconds)
        {
            return Instance.WaitForSeconds(seconds);
        }

        public bool Update(float deltaTime)
        {
            // Remove all ITickables that stopped updating
            int removedElements = yieldInstructions.RemoveAll( x => x.Update(deltaTime));
            return yieldInstructions.Count > 0;
        }
    }
}
