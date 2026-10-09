Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing.Common,System.Drawing.Primitives,System.Private.Windows.GdiPlus,System.Private.Windows.Core -TypeDefinition @"
using System.Drawing;
using System.Drawing.Imaging;
public static class SpriteExtraction {
 public static void Trim(string path) {
  using(var image = new Bitmap(path)) {
   int l=image.Width,t=image.Height,r=-1,b=-1;
   for(int y=0;y<image.Height;y++) for(int x=0;x<image.Width;x++) if(image.GetPixel(x,y).A>8) {l=System.Math.Min(l,x);t=System.Math.Min(t,y);r=System.Math.Max(r,x);b=System.Math.Max(b,y);}
   if(r<l) throw new System.Exception("Empty sprite: "+path);
   l=System.Math.Max(0,l-2);t=System.Math.Max(0,t-2);r=System.Math.Min(image.Width-1,r+2);b=System.Math.Min(image.Height-1,b+2);
   using(var crop=image.Clone(new Rectangle(l,t,r-l+1,b-t+1),PixelFormat.Format32bppArgb)) crop.Save(path+".trim.png",ImageFormat.Png);
  }
  System.IO.File.Copy(path+".trim.png",path,true);System.IO.File.Delete(path+".trim.png");
 }
}
"@
Get-ChildItem -LiteralPath Assets/ShopSystem/Sprites/IllustratedCream/PNG -Filter *.png | Where-Object Name -ne 'village-background.png' | ForEach-Object { [SpriteExtraction]::Trim($_.FullName) }

