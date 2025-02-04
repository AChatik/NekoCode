using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Converters;

namespace NekoCode
{
    public class NekodeAnimation
    {
        public List<NekodeSprite> Sprites;
        public double Duration;
        public DateTime StartTime;
        double delay = 0;
        public bool IsLoop = false;
        public bool IsOnGoing = false;

        public NekodeAnimation(double duration, List<NekodeSprite> sprites, bool loop = false)
        {
            IsLoop = loop;
            Sprites = sprites;
            Duration = duration;
            delay = Duration / Sprites.Count;
        }
        public void Start()
        {
            if (Sprites.Count == 0)
            {
                throw new Exception("Animation is empty");
            }
            StartTime = DateTime.Now;
            IsOnGoing = true;
        }

        public NekodeSprite GetCurrentSprite()
        {
            double runTime = DateTime.Now.Subtract(StartTime).TotalSeconds;
            if (runTime > Duration)
            {
                if (IsLoop)
                {
                    Start();
                    return Sprites[0];
                }
                IsOnGoing = false;
                return Sprites[Sprites.Count - 1];
            }
            else
            {
                return Sprites[(int)Math.Floor(runTime / delay)];
            }
        }

    }
}
