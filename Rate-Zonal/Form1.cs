// Rate-Zonal calculator. Originally written by Russ Renzas in 2013.
// Open source, use as you will, attribution preferred but realistically I'm not going to notice. 
// Originally written to determine how to separate different particle types using rate-zonal technique. 
// Very simple program for a very simple purpose. Please verify calculations for your application.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rate_Zonal
{
    public partial class FormRZ : Form
    {
        //Note: PROGRAM DOES NOT USE REAL VELOCITY, IT USES A VISCOSITY-ADJUSTED VISCOSITY (um/sec*cP)
        private double z0cm;
        private double z1cm;
        private double z2cm;
        private double z3cm;
        private double z0cP;
        private double z1cP;
        private double  z2cP;
        private double  z3cP;
        private double  sVel;
        private double  nwVel;
        private double  bVel;
        private double  sSD;
        private double  nwSD;
        private double  bSD;
        private double  Gx;
        private double  tsec;

        public FormRZ()
        {
            InitializeComponent();
            runTubeUpdate();
            runSedimentUpdate();
        }

        private void updateTube(object sender, EventArgs e)
        {
            runTubeUpdate();
            runSedimentUpdate();
        }

        private void runTubeUpdate()
        {
            double zone0 = (double)numericZone0cm.Value;
            double zone1 = (double)numericZone1cm.Value;
            double zone2 = (double)numericZone2cm.Value;
            double zone3 = (double)numericZone3cm.Value;
            double pixelTubeHeight = (double)rectangleTube.Height;
            double zoneSum = zone0+zone1+zone2+zone3;

            rectangleZone0.Height = (int)(zone0 / zoneSum * pixelTubeHeight);
            rectangleZone1.Height = (int)(zone1 / zoneSum * pixelTubeHeight);
            rectangleZone2.Height = (int)(zone2 / zoneSum * pixelTubeHeight);

            rectangleZone1.Top = rectangleZone0.Bottom;
            rectangleZone2.Top = rectangleZone1.Bottom;

            rectangleSmalls.Height = rectangleZone0.Bottom - rectangleZone0.Top;
            rectangleWires.Height = rectangleSmalls.Height;
            rectangleBigs.Height = rectangleSmalls.Height;

        }

        private void runSedimentUpdate()
        {
            z0cm = (double)numericZone0cm.Value;
            z1cm = (double)numericZone1cm.Value;
            z2cm = (double)numericZone2cm.Value;
            z3cm = (double)numericZone3cm.Value;
            z0cP = (double)numericZone0cP.Value;
            z1cP = (double)numericZone1cP.Value;
            z2cP = (double)numericZone2cP.Value;
            z3cP = (double)numericZone3cP.Value;
            sVel = (double)numericSmallsAvgUmSec.Value;
            nwVel = (double)numericWiresAvgUmSec.Value;
            bVel = (double)numericBigsAvgUmSec.Value;
            sSD = (double)numericSmallsStdDevUmSec.Value;
            nwSD = (double)numericWiresStdDevUmSec.Value;
            bSD = (double)numericBigsStdDevUmSec.Value;
            Gx = (double)numericGs.Value;
            tsec = (double)numericTimeSec.Value;

            double sSlow = CalcZposition(0,sVel - sSD);
            double sFast = CalcZposition(z0cm,sVel + sSD);
            double nwSlow = CalcZposition(0,nwVel - nwSD);
            double nwFast = CalcZposition(z0cm,nwVel + nwSD);
            double bSlow = CalcZposition(0,bVel - bSD);
            double bFast = CalcZposition(z0cm,bVel + bSD);

            double zSum = z0cm + z1cm + z2cm + z3cm;
            double pixelTubeHeight = (double)rectangleTube.Height;

            rectangleSmalls.Top = rectangleTube.Top + (int)((sSlow / zSum) * (double)rectangleTube.Height);
            rectangleSmalls.Height = (int)((sFast / zSum) * (double)rectangleTube.Height);
            rectangleWires.Top = rectangleTube.Top + (int)((nwSlow / zSum) * (double)rectangleTube.Height);
            rectangleWires.Height = (int)((nwFast / zSum) * (double)rectangleTube.Height);
            rectangleBigs.Top = rectangleTube.Top + (int)((bSlow / zSum) * (double)rectangleTube.Height);
            rectangleBigs.Height = (int)((bFast / zSum) * (double)rectangleTube.Height);

            if (rectangleSmalls.Top < rectangleTube.Top) rectangleSmalls.Top = rectangleTube.Top;
            if (rectangleWires.Top < rectangleTube.Top) rectangleWires.Top = rectangleTube.Top;
            if (rectangleBigs.Top < rectangleTube.Top) rectangleBigs.Top = rectangleTube.Top;
        }

        private double CalcZposition(double x0, double vel)
        {
            vel = vel / 10000; //convert to cm/sec*CP
            double x = vel * Gx * tsec / z0cP + x0;
            if (x > z0cm) //if true, made it past z0
            {
                double t0 = (z0cm - x0) * z0cP / (vel * Gx); //time to escape z0
                x = vel * Gx / z1cP * (tsec - t0); //distance into z1
                if (x > z1cm) //if true, made it past z1
                {
                    double t1 = z1cm * z1cP / (vel * Gx); //time to escape z1
                    x = vel * Gx / z2cP * (tsec - t0 - t1); //distance into z2
                    if (x > z2cm) //if true, made it past z2
                    {
                        double t2 = z2cm * z2cP / (vel * Gx); //time to escape z2
                        x = vel * Gx / z3cP * (tsec - t0 - t1-t2); //distance into z3
                        if (x > z3cm) return z0cm + z1cm + z2cm + z3cm;
                        else return x + z0cm + z1cm + z2cm;
                    }
                    else return x + z0cm + z1cm;
                }
                else return (x+z0cm);
            }
            else return x;
        }
    }
}
