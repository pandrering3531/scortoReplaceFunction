using System;
using System.Configuration;

namespace STPC.DynamicForms.Web
{
    public class FormInfoSection : ConfigurationSection
    {
        [ConfigurationProperty("maxlength")]
        public MaxLengthElement MaxLength
        {
            get
            {
                return (MaxLengthElement)this["maxlength"];
            }
            set
            { this["maxlength"] = value; }
        }
    }

    public class MaxLengthElement : ConfigurationElement
    {
        [ConfigurationProperty("value", DefaultValue = "255", IsRequired = true)]
        public int Value
        {
            get
            {
                return (int)this["value"];
            }
            set
            {
                this["value"] = value;
            }
        }
    }

    public class DomainInfoSection : ConfigurationSection
    {
        // Create a "domain" element
        [ConfigurationProperty("domain")]
        public DomainElement Domain
        {
            get
            {
                return (DomainElement)this["domain"];
            }
            set
            { this["domain"] = value; }
        }
    }

    public class DomainElement : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "domain.com", IsRequired = true)]
        [StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 25)]
        public String Name
        {
            get
            {
                return (String)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }
    }
}