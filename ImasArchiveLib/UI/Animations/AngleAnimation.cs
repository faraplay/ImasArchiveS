using System;

namespace Imas.UI
{
    [SerialisationDerivedType(7)]
    public class AngleAnimation : StartEndAnimation
    {
        [SerialiseProperty(200)]
        public float StartAngleRad { get; set; }
        [Listed(200)]
        public float StartAngle
        {
            get => (float)(StartAngleRad * 180 / Math.PI);
            set
            {
                StartAngleRad = (float)(value * Math.PI / 180);
            }
        }
        [SerialiseProperty(201)]
        public float EndAngleRad { get; set; }
        [Listed(201)]
        public float EndAngle
        {
            get => (float)(EndAngleRad * 180 / Math.PI);
            set
            {
                EndAngleRad = (float)(value * Math.PI / 180);
            }
        }
    }
}
