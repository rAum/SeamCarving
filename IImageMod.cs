using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeamCarving
{
    interface IImageMod
    {
        RawImage Modify(RawImage rim);
        void ModifyInPlace(ref RawImage rim); 
    }
}
