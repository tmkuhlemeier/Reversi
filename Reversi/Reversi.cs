using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Reversi
{
    public partial class ReversiForm : Form
    {
        //dingen die veranderen als er geklikt wordt op het bord
        public void KlikTekenSteen(object obj, MouseEventArgs mea)
        {
            //in welk veld wordt er geklikt?
            int x = mea.X / veldgrootte;
            int y = mea.Y / veldgrootte;
            string kleur = "", kleurtegenstander = "";

            // is de zet legaal, dan wordt de zet gedaan en gebeurt wat er volgens de spelregels moet gebeuren
            if (isLegaal(x, y) && aantalpas < 2)
            {
                //het aangeklikte veld wordt de kleur van diegene die aan de beurt is
                velden[x, y] = aandebeurt;

                //de kleuren in bepaalde richting(en) worden veranderd
                veranderKleur(velden, x, y);

                // de beurt gaat naar de andere speler
                aandebeurt = aandebeurt * -1;

                if (aandebeurt == 1)
                {
                    kleur = "blauw";
                    kleurtegenstander = "rood";
                }
                else if (aandebeurt == -1)
                {
                    kleur = "rood";
                    kleurtegenstander = "blauw";
                }

                //de waarden van de velden worden opnieuw berekend
                blauwestenen = 0;
                rodestenen = 0;
                legevelden = 0;
                for (int i = 0; i < breedte; i++)
                {
                    for (int j = 0; j < hoogte; j++)
                    {
                        int steen = velden[i, j];
                        if (steen == 1)
                            blauwestenen++;
                        if (steen == -1)
                            rodestenen++;
                        if (steen == 0)
                            legevelden++;
                    }
                }

                //uitkomsten dmv messageboxen aan de gebruiker duidelijk maken
                if (rodestenen == 0)
                {
                    MessageBox.Show("blauw wint", "bericht", MessageBoxButtons.OK);
                    this.Invalidate();
                }
                if (blauwestenen == 0)
                {
                    MessageBox.Show("rood wint", "bericht", MessageBoxButtons.OK);
                    this.Invalidate();
                }

                if (legevelden == 0)
                {
                    if (blauwestenen > rodestenen)
                        MessageBox.Show("blauw wint", "bericht", MessageBoxButtons.OK);
                    else if (rodestenen > blauwestenen)
                        MessageBox.Show("rood wint", "bericht", MessageBoxButtons.OK);
                    else if (blauwestenen == rodestenen)
                        MessageBox.Show("remise", "bericht", MessageBoxButtons.OK);
                    this.Invalidate();
                }

                plaatsHulpStenen();
                int hulpstenen = berekenHulpStenen(veldenkopie);

                // 0 hulpstenen, er moet gepast worden. gebeurt dit twee keer achter elkaar, dan is het spel afgelopen. 
                // degene met de meeste stenen is dan de winnaar
                if (hulpstenen == 0 && legevelden != 0)
                {
                    aantalpas++;
                    if (aantalpas == 2)
                    {
                        if (blauwestenen > rodestenen)
                            MessageBox.Show(kleur + " kan geen zetten doen, blauw heeft gewonnen", "bericht", MessageBoxButtons.OK);
                        else if (rodestenen > blauwestenen)
                            MessageBox.Show(kleur + " kan geen zetten doen, rood heeft gewonnen", "bericht", MessageBoxButtons.OK);
                        else if (blauwestenen == rodestenen)
                            MessageBox.Show(kleur + " kan geen zetten doen, gelijkspel", "bericht", MessageBoxButtons.OK);
                        this.Invalidate();
                    }

                    //in het geval van een keer pas gaat de beurt door naar de andere speler
                    else if (blauwestenen != 0 && rodestenen != 0)
                    {
                        MessageBox.Show(kleur + " kan geen zetten doen, de beurt gaat naar " + kleurtegenstander + ".", "bericht", MessageBoxButtons.OK);
                        aandebeurt *= -1;
                    }
                }
                //bord wordt opnieuw getekend
                Bord.Invalidate();
            }
        }

        //in bepaalde richting(en) veranderen van de kleur, parameter velden array en x en y van veld van click
        public void veranderKleur(int[,] a, int x, int y)
        {
            for (int n = 0; n < horizontaal.Count; n++) //tot de lengte van de list van richtingen // wat als verschillend?
            {
                int dx = horizontaal[n];
                int dy = verticaal[n];
                int nieuwx = x + dx; int nieuwy = y + dy;
                while (a[nieuwx, nieuwy] != 0 && a[nieuwx, nieuwy] != aandebeurt) // van hoever tot hoever
                {
                    a[nieuwx, nieuwy] = aandebeurt;
                    nieuwx += dx;
                    nieuwy += dy;
                }
            }
        }

        public void Kliknieuwspel(object sender, EventArgs e)
        {
            //reset het scherm
            //deze verwijdert alle stenen
            //plaatst de beginstenen
            verwijderStenen(velden);

            plaatsBeginStenen(velden);

            aandebeurt = 1;
            aantalpas = 0;
            waarde = 0;
            aantalklikhelp = 0; //kan ook met bool?
            Bord.Invalidate();
        }

        public void Klikhelp(object sender, EventArgs e)
        {
            aantalklikhelp++;
            //laat legale zetten zien
            if (aantalpas < 2)
            {
                if (aantalklikhelp % 2 == 1)
                    waarde = 3; //dan worden de hulpstenen zichtbaar
                else
                    waarde = 0; // dan worden de hulpstenen onzichtbaar

                Bord.Invalidate();
            }
        }

        // welke richtingen zijn er mogelijk uit een punt
        private bool checkVoorRichting(int x, int y, int dx, int dy)
        {
            int tegenstanderkleur = aandebeurt * -1;
            // valt de richting buiten het bord, dan is deze richting false
            if (x + dx > breedte - 1 || x + dx < 0)
                return false;
            if (y + dy > hoogte - 1 || y + dy < 0)
                return false;
            // leidt de richting tot een naburige steen die de kleur van de tegenstander heeft, ga dan verder, anders false
            if (velden[x + dx, y + dy] == tegenstanderkleur)
            {
                int nieuwx = x + dx; int nieuwy = y + dy;
                // zolang het veld niet leeg is, ga verder in die richting
                while (velden[nieuwx, nieuwy] != 0)
                {
                    // kom je een steen van de eigen kleur tegen, dan is deze richting juist, anders ga verder
                    if (velden[nieuwx, nieuwy] == aandebeurt)
                    {
                        return true;
                    }
                    nieuwx += dx;
                    nieuwy += dy;
                    // valt de richting buiten het bord, dan is deze richting false
                    if (nieuwx > breedte - 1 || nieuwx < 0)
                        return false;
                    if (nieuwy > hoogte - 1 || nieuwy < 0)
                        return false;
                }
            }
            return false;
        }


        // waarde hiervan gebruiken in teknmethode
        private bool isLegaal(int x, int y)
        {
            int nieuwesteen = aandebeurt * -1;

            // is het wel leeg
            if (velden[x, y] != 0 && velden[x, y] != 3)
                return false;
            //maak de lists leeg
            horizontaal.Clear();
            verticaal.Clear();
            // is er ook een legale richting, zo ja voeg dx en dy toe aan de lists
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (checkVoorRichting(x, y, dx, dy))
                    {
                        horizontaal.Add(dx);
                        verticaal.Add(dy);
                    }
                }
            }
            // bevat de list element(en), dan is de zet legaal, anders false
            if (horizontaal.Count != 0)
                return true;
            else
                return false;
        }

        //hulpmethoden

        // deze verwijdert alle stenen op het bord // kan met foreach?
        public void verwijderStenen(int[,] a)
        {
            for (int x = 0; x < breedte; x++)
            {
                for (int y = 0; y < hoogte; y++)
                {

                    a[x, y] = 0;
                }
            }
        }

        // deze bepaalt de plaats van de beginstenen
        public void plaatsBeginStenen(int[,] a)
        {
            int middenx = breedte/ 2;
            int middeny = hoogte/ 2;
            a[middenx - 1, middeny] = -1;
            a[middenx - 1, middeny - 1] = 1;
            a[middenx, middeny - 1] = -1;
            a[middenx, middeny] = 1;
        }

        // deze bepaalt de plaats van de hulpstenen
        public void plaatsHulpStenen()
        {
            for (int x = 0; x < breedte; x++)
            {
                for (int y = 0; y < hoogte; y++)
                {
                    if (isLegaal(x, y))
                    {
                        aantalpas = 0; //nodig?
                        veldenkopie[x, y] = waarde;
                    }
                }
            }
        }

        // berekent hoeveel hulpstenen er zijn en returnt deze
        public int berekenHulpStenen(int[,] b) //breedte? hoogte?
        {
            int hulpstenen = 0;
            for (int x = 0; x < breedte; x++)
            {
                for (int y = 0; y < hoogte; y++)
                {
                    if (isLegaal(x, y))
                    {
                        hulpstenen++;
                    }
                }
            }
            return hulpstenen;
        }
    }
}

