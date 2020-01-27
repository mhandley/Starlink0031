using UnityEngine;

namespace QuickPrimitives
{
    public class QcBaseProperties
    {
        public Vector3 offset;

        public bool genTextureCoords = true;
        public bool addCollider = true;

        public void CopyFrom(QcBaseProperties source)
        {
            this.offset = source.offset;
            this.genTextureCoords = source.genTextureCoords;
            this.addCollider = source.addCollider;
        }
    }
}
