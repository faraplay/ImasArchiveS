using System;

namespace Imas.UI
{
    [SerialisationDerivedType(5)]
    public class RotatableGroupControl : GroupControl
    {
        [SerialiseProperty(200)]
        public float AngleRad { get; set; }
        [Listed(200)]
        public float Angle
        {
            get => (float)(AngleRad * 180 / Math.PI);
            set
            {
                AngleRad = (float)(value * Math.PI / 180);
            }
        }

        [SerialiseProperty(201)]
        [Listed(201)]
        public float E2 { get; set; }
        [SerialiseProperty(201)]
        [Listed(202)]
        public float E3 { get; set; }

    }
}
