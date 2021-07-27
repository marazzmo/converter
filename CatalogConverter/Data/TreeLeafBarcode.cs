namespace CatalogConverter.Data
{
    using System;
    using System.Globalization;
    using System.Xml.Serialization;

    [Serializable]
    public class TreeLeafBarcode
    {
        private decimal multiplierVal = decimal.One;
        private bool isWeightVal;

        [XmlAttribute("Value")]
        public string Value { get; set; }

        [XmlAttribute("Multiplier")]
        public string MultiplierAttribute
        {
            get
            {
                return this.Multiplier.ToString((IFormatProvider)CultureInfo.InvariantCulture);
            }

            set
            {
                decimal result;
                if (!decimal.TryParse(value, NumberStyles.AllowDecimalPoint, (IFormatProvider)CultureInfo.InvariantCulture, out result))
                {
                    return;
                }

                this.Multiplier = result;
            }
        }

        [XmlAttribute("IsWeight")]
        public string IsWeightAttribute
        {
            get
            {
                return this.IsWeight.ToString();
            }

            set
            {
                bool result;
                if (!bool.TryParse(value, out result))
                {
                    return;
                }

                this.IsWeight = result;
            }
        }

        [XmlIgnore]
        public bool IsWeight
        {
            get
            {
                return this.isWeightVal;
            }

            set
            {
                this.isWeightVal = value;
            }
        }

        [XmlIgnore]
        public decimal Multiplier
        {
            get
            {
                return this.multiplierVal;
            }

            set
            {
                this.multiplierVal = value;
            }
        }
    }
}

