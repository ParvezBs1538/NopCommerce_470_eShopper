namespace NopStation.Plugin.Misc.QuoteCart.Domain;

public enum AttributeControlType
{
    /// <summary>
    /// Dropdown list
    /// </summary>
    DropdownList = 1,

    /// <summary>
    /// Radio list
    /// </summary>
    RadioList = 2,

    /// <summary>
    /// Check boxes
    /// </summary>
    Checkboxes = 3,

    /// <summary>
    /// TextBox
    /// </summary>
    TextBox = 4,

    /// <summary>
    /// Multiline textbox
    /// </summary>
    MultilineTextbox = 10,

    /// <summary>
    /// Datepicker
    /// </summary>
    Datepicker = 20,

    /// <summary>
    /// File upload control
    /// </summary>
    FileUpload = 30,

    /// <summary>
    /// Color squares
    /// </summary>
    ColorSquares = 40,

    /// <summary>
    /// Image squares
    /// </summary>
    ImageSquares = 45,

    /// <summary>
    /// Read-only check boxes
    /// </summary>
    ReadonlyCheckboxes = 50
}
