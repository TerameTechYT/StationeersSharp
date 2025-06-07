namespace StationeersLibrary.Args;

public class MainMenuPageEnabledArgs {
    public string Page { get; private set; }

    public MainMenuPageEnabledArgs(string page) {
        this.Page = page;
    }
}
