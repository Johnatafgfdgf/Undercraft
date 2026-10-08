#!/usr/bin/env python3
"""Generate Undercraft's production 2D Steve sprite frames from a vanilla 64x64 wide-arm skin.

The runtime stays 2D. The generator uses Minecraft player proportions and a
sinusoidal alternating arm/leg gait instead of hand-drawn body rectangles.
Minecraft assets are supplied locally and are not committed.
"""
from PIL import Image
from pathlib import Path
import argparse, math

UV = {
    "head": {"front":(8,8,16,16),"back":(24,8,32,16),"left":(16,8,24,16),"right":(0,8,8,16)},
    "body": {"front":(20,20,28,32),"back":(32,20,40,32),"left":(28,20,32,32),"right":(16,20,20,32)},
    "rarm": {"front":(44,20,48,32),"back":(52,20,56,32),"left":(48,20,52,32),"right":(40,20,44,32)},
    "rleg": {"front":(4,20,8,32),"back":(12,20,16,32),"left":(8,20,12,32),"right":(0,20,4,32)},
    "larm": {"front":(36,52,40,64),"back":(44,52,48,64),"left":(40,52,44,64),"right":(32,52,36,64)},
    "lleg": {"front":(20,52,24,64),"back":(28,52,32,64),"left":(24,52,28,64),"right":(16,52,20,64)}
}
SCALE=2
CANVAS=(48,76)
CX=24

def main():
    ap=argparse.ArgumentParser()
    ap.add_argument("skin")
    ap.add_argument("output")
    args=ap.parse_args()
    skin=Image.open(args.skin).convert("RGBA")
    out=Path(args.output); out.mkdir(parents=True,exist_ok=True)

    def part(name,d):
        im=skin.crop(UV[name][d])
        return im.resize((im.width*SCALE,im.height*SCALE),Image.Resampling.NEAREST)

    def rotate_top(im,deg):
        pad=max(im.width,im.height)
        c=Image.new("RGBA",(im.width+pad*2,im.height+pad*2),(0,0,0,0))
        c.alpha_composite(im,(pad,pad))
        pivot=(pad+im.width/2,pad)
        w,h=c.size
        shifted=Image.new("RGBA",c.size,(0,0,0,0))
        shifted.alpha_composite(c,(int(w/2-pivot[0]),int(h/2-pivot[1])))
        r=shifted.rotate(deg,resample=Image.Resampling.NEAREST,center=(w/2,h/2))
        box=r.getbbox()
        return r.crop(box) if box else r

    def paste(c,im,x,y):
        c.alpha_composite(im,(int(x-im.width/2),int(y)))

    def frame(d,phase,state):
        c=Image.new("RGBA",CANVAS,(0,0,0,0))
        t=phase*math.tau
        if state=="idle": arm=leg=0; bob=0
        elif state=="jump": arm=-18; leg=10; bob=-2
        elif state=="use": arm=-65; leg=0; bob=0
        else:
            arm=math.sin(t)*32
            leg=-math.sin(t)*28
            bob=round(abs(math.sin(t))*1.5)
        arm_sep=4 if d in ("left","right") else 12
        leg_sep=2 if d in ("left","right") else 5
        paste(c,rotate_top(part("larm",d),arm),CX-arm_sep,25+bob)
        paste(c,rotate_top(part("lleg",d),leg),CX-leg_sep,42+bob)
        paste(c,part("body",d),CX,24+bob)
        paste(c,part("head",d),CX,8+bob)
        paste(c,rotate_top(part("rleg",d),-leg),CX+leg_sep,42+bob)
        paste(c,rotate_top(part("rarm",d),-arm),CX+arm_sep,25+bob)
        return c

    for d in ("front","back","left","right"):
        for i in range(8):
            frame(d,i/8,"walk").save(out/f"spr_uc10_steve_{d}_{i}.png")
        frame(d,0,"idle").save(out/f"spr_uc10_steve_{d}_idle.png")
        frame(d,0,"jump").save(out/f"spr_uc10_steve_{d}_jump.png")
        frame(d,0,"use").save(out/f"spr_uc10_steve_{d}_use.png")

if __name__=="__main__":
    main()
