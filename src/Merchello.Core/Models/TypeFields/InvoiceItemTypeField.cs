﻿using Merchello.Core.Configuration.Outline;

namespace Merchello.Core.Models.TypeFields
{
    /// <summary>
    /// The type of a invoice line item
    /// </summary>
    public class InvoiceItemTypeField : TypeFieldProxyBase
    {         
        /// <summary>
        /// Catalog product sales
        /// </summary>
        public static ITypeField Product
        {
            get { return TypeFieldProvider.GetTypeField(MerchelloType.InvoiceItemProduct); }
        }

        /// <summary>
        /// A standard charge or fee
        /// </summary>
        public static ITypeField Charge
        {
            get { return TypeFieldProvider.GetTypeField(MerchelloType.InvoiceItemCharge); }
        }

        /// <summary>
        /// A shipping specific charge
        /// </summary>
        public static ITypeField Shipping
        {
            get { return TypeFieldProvider.GetTypeField(MerchelloType.InvoiceItemShipping); }
        }

        /// <summary>
        /// A tax related charge
        /// </summary>
        public static ITypeField Tax
        {
            get { return TypeFieldProvider.GetTypeField(MerchelloType.InvoiceItemTax); }
        }

        /// <summary>
        /// A credit
        /// </summary>
        public static ITypeField Credit
        {
            get { return TypeFieldProvider.GetTypeField(MerchelloType.InvoiceItemCredit); }
        }

        /// <summary>
        /// Returns a custom invoice item types or a NullTypeField
        /// </summary>
        /// <param name="alias">The alias of the custom invoice item type</param>
        /// <returns>An object of <see cref="ITypeField"/></returns>
        public new static ITypeField Custom(string alias)
        {
            return GetTypeField(Items[alias]);
        }


        private static TypeFieldCollection Items
        {
            get { return Fields.InvoiceItem; }
        }

    }
}