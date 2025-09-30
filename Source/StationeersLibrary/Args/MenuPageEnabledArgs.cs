#region

#endregion

namespace StationeersLibrary.Args;

public class MenuPageEnabledArgs {
    public string Page { get; private set; }

    public MenuPageEnabledArgs(string page) => this.Page = page;
}
