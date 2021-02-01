using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace SchetsEditor
{
    
    [Serializable]
    public struct DrawInstuction
    {

        public DrawInstuction(ElementType elType, Color elKleur, Point elStartPunt, Point elEindPunt, int elLijnDikte = 3) : this()
        {
            elementType = elType;
            kleur = elKleur;
            startPunt = elStartPunt;
            eindPunt = elEindPunt;
            lijnDikte = elLijnDikte;
        }

        public DrawInstuction(ElementType elType, Color elKleur, Point elStartPunt, Font elFont, char elChar) : this()
        {
            elementType = elType;
            kleur = elKleur;
            startPunt = elStartPunt;
            font = elFont;
            letter = elChar;
        }

        public DrawInstuction(ElementType elType, Color elKleur, List<Point> elPunten, int elLijnDikte = 3) : this()
        {
            elementType = elType;
            kleur = elKleur;
            puntenVanLijn = elPunten;
            lijnDikte = elLijnDikte;
        }


        public ElementType elementType { get; set; }
        public Point startPunt { get; set; }
        public Point eindPunt { get; set; }
        public int lijnDikte { get; set; }
        public char letter { get; set; }
        public List<Point> puntenVanLijn { get; set; }

        //color should be ignored by xml serializer because it can't normally be serialized.
        [XmlIgnore]
        public Color kleur { get; set; }
        //Fix the color serialization. Taken from: https://stackoverflow.com/a/12101050/8902440
        [XmlElement("kleur"), Browsable(false)]
        public int kleurAsArgb
        {
            get { return kleur.ToArgb(); }
            set { kleur = Color.FromArgb(value); }
        }

        //font should be ingored by xml serializer because it can't normally be serialized.
        [XmlIgnore()]
        public Font font { get; set; }
        //Fix the font serialization. Taken from: https://stackoverflow.com/a/34934422/8902440
        [XmlElement("font"), Browsable(false)]
        public string FontSerialize
        {
            get { return TypeDescriptor.GetConverter(typeof(Font)).ConvertToInvariantString(font); }
            set { font = TypeDescriptor.GetConverter(typeof(Font)).ConvertFromInvariantString(value) as Font; }
        }

        public override string ToString() => $"Type: {elementType.ToString()}, kleur: {kleur.ToString()}, lijndikte: {lijnDikte.ToString()}, startpunt: {startPunt.ToString()}, eindpunt: {eindPunt.ToString()};";
        
        public Brush CreateBrush() => new SolidBrush(kleur);
       
        public Pen CreatePen() => TweepuntTool.MaakPen(CreateBrush(), lijnDikte);
        public Rectangle ToRectangle() => TweepuntTool.Punten2Rechthoek(startPunt, eindPunt);
    }
}
