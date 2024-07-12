namespace NopStation.Plugin.Widgets.SEOExpert
{
    public static class SEOExpertDefaults
    {
        public static string OpenAIAuthKeyPrefix => "Bearer ";

        public static string AdditionalCommandproduct => "generate MetaTitle, MetaDescription, MetaKeywords for this product sent in JSON Format for SEO purpose. Please response in Json format also. for this product the keywords will be MetaTitle, MetaDescription and MetaKeywords and also return the Id and Sku field what i have sent. Please use most common words people use for searching ecommerce products and MetaDescription with a long description";
        public static string AdditionalCommandcategory => "generate MetaTitle, MetaDescription, MetaKeywords for this category sent in JSON Format for SEO purpose. Please response in Json format also. for this category the keywords will be MetaTitle, MetaDescription and MetaKeywords and also return the Id field what i have sent. Please use most common words people use for searching ecommerce category and MetaDescription with a long description";
        public static string AdditionalCommandmanufacturer => "generate MetaTitle, MetaDescription, MetaKeywords for this manufacturer sent in JSON Format for SEO purpose. Please response in Json format also. for this manufacturer the keywords will be MetaTitle, MetaDescription and MetaKeywords and also return the Id field what i have sent. Please use most common words people use for searching ecommerce manufacturer and MetaDescription with a long description";
        public static string AdditionalStoreNameCommand => "My Store Name is {0}.";
    }
}
