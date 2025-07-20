using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowCAD_core
{
    internal class BoyProfilCircle
    {
        string bacaNo;
        string kapakKot;
        string girişKot;
        string akarKot;
        double yKoordinat;
        double xKoordinat;
        int cap;

        public BoyProfilCircle()
        {
            bacaNo = string.Empty;
            kapakKot = string.Empty;
            girişKot = string.Empty;
            akarKot = string.Empty;
            yKoordinat = 0.0;
            xKoordinat = 0.0;
            cap = 0;
        }

        public BoyProfilCircle(string bacaNo, string kapakKot, string girişKot, string akarKot, double yKoordinat, double xKoordinat, int cap)
        {
            this.bacaNo = bacaNo;
            this.kapakKot = kapakKot;
            this.girişKot = girişKot;
            this.akarKot = akarKot;
            this.yKoordinat = yKoordinat;
            this.xKoordinat = xKoordinat;
            this.cap = cap;
        }

        public string BacaNo
        {
            get { return bacaNo; }
            set { bacaNo = value; }
        }
        public string KapakKot
        {
            get { return kapakKot; }
            set { kapakKot = value; }
        }
        public string GirişKot
        {
            get { return girişKot; }
            set { girişKot = value; }
        }
        public string AkarKot
        {
            get { return akarKot; }
            set { akarKot = value; }
        }
        public double YKoordinat
        {
            get { return yKoordinat; }
            set { yKoordinat = value; }
        }
        public double XKoordinat
        {
            get { return xKoordinat; }
            set { xKoordinat = value; }
        }
        public int Cap
        {
            get { return cap; }
            set { cap = value; }
        }
    }
}
