// System
using System;
using System.Reflection;

// Unity
using UnityEngine;

namespace GUPS.EasyPooling.Shapes
{
    /// <summary>
    /// Behaves like a Unity Rect but in 3D space. A 3D Box is defined by X, Y and Z position, width, height, depth.
    /// </summary>
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Box
    {
        [SerializeField]
        private float x;

        public float X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        [SerializeField]
        private float y;

        public float Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        [SerializeField]
        private float z;

        public float Z
        {
            get { return this.z; }
            set { this.z = value; }
        }

        [SerializeField]
        private float width;
        
        public float Width
        {
            get { return this.width; }
            set { this.width = value; }
        }

        [SerializeField]
        private float height;

        public float Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        [SerializeField]
        private float depth;

        public float Depth
        {
            get { return this.depth; }
            set { this.depth = value; }
        }

        public Vector3 Position
        {
            get { return new Vector3(this.x, this.y, this.z); }
            set
            {
                this.x = value.x;
                this.y = value.y;
                this.z = value.z;
            }
        }

        public Vector3 Size
        {
            get { return new Vector3(this.width, this.height, this.depth); }
            set
            {
                this.width = value.x;
                this.height = value.y;
                this.depth = value.z;
            }
        }

        public Vector3 Center
        {
            get { return new Vector3(this.x + (this.width / 2), this.y + (this.height / 2), this.z + (this.depth / 2)); }
            set
            {
                this.x = value.x - (this.width / 2);
                this.y = value.y - (this.height / 2);
                this.z = value.z - (this.depth / 2);
            }
        }

        public Box()
        {
            this.x = 0;
            this.y = 0;
            this.z = 0;
            this.width = 0;
            this.height = 0;
            this.depth = 0;
        }

        public Box(float _X, float _Y, float _Z, float _Width, float _Height, float _Depth)
        {
            this.x = _X;
            this.y = _Y;
            this.z = _Z;
            this.width = _Width;
            this.height = _Height;
            this.depth = _Depth;
        }

        public Box(Vector3 _Position, Vector3 _Size)
        {
            this.x = _Position.x;
            this.y = _Position.y;
            this.z = _Position.z;
            this.width = _Size.x;
            this.height = _Size.y;
            this.depth = _Size.z;
        }

        public Box(Box _Box)
        {
            this.x = _Box.x;
            this.y = _Box.y;
            this.z = _Box.z;
            this.width = _Box.width;
            this.height = _Box.height;
            this.depth = _Box.depth;
        }

        public bool Contains(Vector3 _Point)
        {
            return _Point.x >= this.x && _Point.x <= this.x + this.width
                && _Point.y >= this.y && _Point.y <= this.y + this.height
                && _Point.z >= this.z && _Point.z <= this.z + this.depth;
        }

        public bool Overlaps(Box _Box)
        {
            return this.x < _Box.x + _Box.width && this.x + this.width > _Box.x
                && this.y < _Box.y + _Box.height && this.y + this.height > _Box.y
                && this.z < _Box.z + _Box.depth && this.z + this.depth > _Box.z;
        }
    }
}
