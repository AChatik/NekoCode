using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NekoCode
{
    internal class NekodeManager
    {
        public Queue<NekodeAnimation> AnimationsQueue = new Queue<NekodeAnimation>();
        public Image SpriteImage;
        public NekodeAnimation CurrentAnimation = null;
        public NekodeManager(Image SpriteImage)
        {
            this.SpriteImage = SpriteImage;
        }

        public void ApplyAnimation(NekodeAnimation animation)
        {

        }
        public void ApplySprite(NekodeSprite sprite)
        {

        }
        public void AddAnimation(NekodeAnimation animation)
        {
            AnimationsQueue.Enqueue(animation);
        }
        public void Update()
        {
            if (CurrentAnimation != null)
            {
                if (CurrentAnimation.IsOnGoing)
                {
                    ApplySprite(CurrentAnimation.GetCurrentSprite());
                }
                else
                {
                    CurrentAnimation = null;
                }
            }
            else
            {
                CurrentAnimation = AnimationsQueue.Dequeue();
                CurrentAnimation.Start();
            }
        }
    }
}
