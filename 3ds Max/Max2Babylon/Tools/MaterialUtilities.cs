using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;

namespace Max2Babylon
{
    // todo: MaterialUtilities class should be splitted as the warpper contains more idwrapper then the materials one

    public static class MaterialUtilities
    {
        public static bool IsMaterialAssignedInScene(IMtl mtl)
        {
            for (int i = 0; i < Loader.Core.SceneMtls.Count; i++)
            {
                IMtl sceneMtl = null;
#if MAX2016
                sceneMtl = (IMtl)Loader.Core.SceneMtls[new IntPtr(i)];
#else
                sceneMtl = (IMtl)Loader.Core.SceneMtls[i];
#endif
                if (sceneMtl != null && mtl.GetNativeHandle() == sceneMtl.GetNativeHandle()) return true;
            }
            return false;
        }

        public static IntPtr SlateMtlEditorHwnd
        {
            get
            {
                IntPtr mtlPtr = IntPtr.Zero;
                NativeMethods.EnumWindows(
                    (IntPtr hwnd, IntPtr lparam) => {
                        if (HwndIsSlateMtlEditor(hwnd))
                        {
                            mtlPtr = hwnd;
                            return false;
                        }
                        return true;
                    }, IntPtr.Zero);
                return mtlPtr;
            }
        }

        private static bool HwndIsSlateMtlEditor(IntPtr hwnd)
        {
            return getHwndTitle(hwnd) == "Slate Material Editor";
        }

        private static String getHwndTitle(IntPtr hwnd)
        {
            int textLength = NativeMethods.GetWindowTextLength(hwnd);
            StringBuilder windowText = new StringBuilder(textLength + 1);
            if (NativeMethods.GetWindowText(hwnd, windowText, windowText.Capacity) > 0)
                return windowText.ToString();
            else
                return String.Empty;
        }

        /// <summary>
        /// Retrieve current selected material in slate material editor
        /// </summary>
        /// <returns></returns>
        public static IMtl GetSelectedMaterial()
        {
            string mxs = "viewNode = sme.GetView (sme.activeView)";
            mxs += "\r\n" + "smeSelMats = #()";
            mxs += "\r\n" + "if (trackViewNodes[#sme][(sme.activeView)] != undefined) then(";
            mxs += "\r\n" + "for n = 1 to trackViewNodes[#sme][(sme.activeView)].numSubs do (";
            mxs += "\r\n" + "m = trackViewNodes[#sme][(sme.activeView)][n].reference";
            mxs += "\r\n" + "b = viewNode.GetNodeByRef m";
            mxs += "\r\n" + "if b.selected do append smeSelMats m)";
            mxs += "\r\n" + "smeSelMats[1])";
            
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
#if MAX2015 || MAX2016|| MAX2017 || MAX2018
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal);
#else
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
#endif
            IMtl result = null;
            try
            {
                result = mxsRetVal.Mtl;
            }
            catch (Exception e)
            {
                //do nothing ,just retun a null material
            }

            return result;
        }

        public static float GetFloatMaterialProperty(this IIGameMaterial material, string propName, int key, IInterval interval )
        {
            float result = 0;
            for (int i = 0; i < material.IPropertyContainer.NumberOfProperties; ++i)
            {
                IIGameProperty property = material.IPropertyContainer.GetProperty(i);

                if (property == null)
                    continue;

                
                string propertyName = property.Name.ToUpperInvariant();
                string targetProp = propName.ToUpperInvariant();
                if (propertyName == targetProp)
                {
                    
                    property.GetPropertyValue(ref result, key, true);
                }
            }

            return result;
        }

        // We require a separate struct, because the IClass_ID does not implement GetHashCode etc. to work with dictionaries
    public struct ClassIDWrapper : IEquatable<ClassIDWrapper>
    {
        public static readonly ClassIDWrapper XRef_Material = new ClassIDWrapper(0x272c0d4b, 0x432a414b);
        public static readonly ClassIDWrapper Advanced_Lighting_Override_Material = new ClassIDWrapper(0x2914493d, 0x6cff42f7);
        public static readonly ClassIDWrapper Morpher_Material = new ClassIDWrapper(0x4b9937e0, 0x3a1c3da4);
        public static readonly ClassIDWrapper Architectural_Material = new ClassIDWrapper(0x13d11bbe, 0x691e3037);
        public static readonly ClassIDWrapper Autodesk_Generic_Material = new ClassIDWrapper(0x1ed415e4, 0x213daaf8);
        public static readonly ClassIDWrapper Ink_n_Paint_Material = new ClassIDWrapper(0x01a8169a, 0x4d3960a5);
        public static readonly ClassIDWrapper Map_to_Material_Conversion = new ClassIDWrapper(0x48e04183, 0xa129081c);
        public static readonly ClassIDWrapper Standard_Material = new ClassIDWrapper(0x00000002, 0x00000000);
        public static readonly ClassIDWrapper Multi_Sub_Object_Material = new ClassIDWrapper(0x00000200, 0x00000000);
        public static readonly ClassIDWrapper Double_Sided_Material = new ClassIDWrapper(0x00000210, 0x00000000);
        public static readonly ClassIDWrapper Blend_Material = new ClassIDWrapper(0x00000250, 0x00000000);
        public static readonly ClassIDWrapper Matte_Shadow_Material = new ClassIDWrapper(0x00000260, 0x00000000);
        public static readonly ClassIDWrapper Top_Bottom_Material = new ClassIDWrapper(0x00000100, 0x00000000);
        public static readonly ClassIDWrapper Composite_Material = new ClassIDWrapper(0x61dc0cd7, 0x13640af6);
        public static readonly ClassIDWrapper Shell_Material = new ClassIDWrapper(0x00000255, 0x00000000);
        public static readonly ClassIDWrapper Physical_Material = new ClassIDWrapper(0x3d6b1cec, 0xdeadc001);
        public static readonly ClassIDWrapper Raytrace_Material = new ClassIDWrapper(0x27190ff4, 0x329b106e);
        public static readonly ClassIDWrapper Shellac_Material = new ClassIDWrapper(0x46ee536a, 0x00000000);
        public static readonly ClassIDWrapper mental_ray_Material = new ClassIDWrapper(0x6926ba21, 0x7a10aca5);
        public static readonly ClassIDWrapper Map_to_Material = new ClassIDWrapper(0x8ccdf7bc, 0x72928e19);
        public static readonly ClassIDWrapper DirectX_Shader_Material = new ClassIDWrapper(0x0ed995e4, 0x6133daf2);
        public static readonly ClassIDWrapper Ray_Switch_Shader_Material = new ClassIDWrapper(0x7e73161f, 0x4c074e86);
        public static readonly ClassIDWrapper Lambert_Material = new ClassIDWrapper(0x7e73161f, 0xa80b5727);
        public static readonly ClassIDWrapper Standard_Surface_Material = new ClassIDWrapper(0x7e73161f, 0x62f74b4c);
        public static readonly ClassIDWrapper Standard_Hair_Material = new ClassIDWrapper(0x7e73161f, 0xa964c158);
        public static readonly ClassIDWrapper Car_Paint_Material = new ClassIDWrapper(0x7e73161f, 0x770d4485);
        public static readonly ClassIDWrapper Mix_Shader_Material = new ClassIDWrapper(0x7e73161f, 0x4f30a69d);
        public static readonly ClassIDWrapper Atmosphere_Volume_Material = new ClassIDWrapper(0x7e73161f, 0x57215188);
        public static readonly ClassIDWrapper Fog_Material = new ClassIDWrapper(0x7e73161f, 0x4659b384);
        public static readonly ClassIDWrapper Standard_Volume_Material = new ClassIDWrapper(0x7e73161f, 0xac0b525b);
        public static readonly ClassIDWrapper AOV_Write_Float_Material = new ClassIDWrapper(0x7e73161f, 0x8cff673a);
        public static readonly ClassIDWrapper AOV_Write_Int_Material = new ClassIDWrapper(0x7e73161f, 0x8b688625);
        public static readonly ClassIDWrapper AOV_Write_RGB_Material = new ClassIDWrapper(0x7e73161f, 0x925f862f);
        public static readonly ClassIDWrapper Matte_Material = new ClassIDWrapper(0x7e73161f, 0xba66a526);
        public static readonly ClassIDWrapper Passthrough_Material = new ClassIDWrapper(0x7e73161f, 0x625bb28f);
        public static readonly ClassIDWrapper Switch_Shader_Material = new ClassIDWrapper(0x7e73161f, 0xa844c228);
        public static readonly ClassIDWrapper Two_Sided_Material = new ClassIDWrapper(0x7e73161f, 0x7ffd6281);

        public static readonly ClassIDWrapper Editable_Poly = new ClassIDWrapper(469250957, 422535320);
        public static readonly ClassIDWrapper Sphere = new ClassIDWrapper(17,0);
        public static readonly ClassIDWrapper TargetCamera = new ClassIDWrapper(4098, 0);

        private uint partA, partB;
        public ClassIDWrapper(IClass_ID classID) { partA = classID.PartA; partB = classID.PartB; }
        public ClassIDWrapper(uint partA, uint partB) { this.partA = partA; this.partB = partB; }
        
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            ClassIDWrapper other = (ClassIDWrapper)obj;
            return Equals(other);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + partA.GetHashCode();
                hash = hash * 23 + partB.GetHashCode();
                return hash;
            }
        }

        public bool Equals(ClassIDWrapper other)
        {
            return partA.Equals(other.partA) && partB.Equals(other.partB);
        }
        public bool Equals(IClass_ID other)
        {
            return partA.Equals(other.PartA) && partB.Equals(other.PartB);
        }

        public static bool operator ==(ClassIDWrapper lhs, ClassIDWrapper rhs)
        {
            return lhs.Equals(rhs);
        }
        public static bool operator !=(ClassIDWrapper lhs, ClassIDWrapper rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
    }


}
