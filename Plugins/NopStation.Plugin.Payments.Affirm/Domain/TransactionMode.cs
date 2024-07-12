namespace NopStation.Plugin.Payments.Affirm.Domain
{
    /// <summary>
    /// <para>Represents Authorize.Net payment processor transaction mode</para>
    /// <para>Author: nopStation team</para>
    /// </summary>
    public enum TransactionMode
    {
        /// <summary>
        /// Authorize
        /// </summary>
        Authorize = 1,
        /// <summary>
        /// Authorize and capture
        /// </summary>
        AuthorizeAndCapture = 2
    }
}
