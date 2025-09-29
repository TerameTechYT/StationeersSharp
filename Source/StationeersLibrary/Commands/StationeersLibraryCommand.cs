#region

using System.Web;

#endregion


namespace StationeersLibrary.Commands;

public class StationeersLibraryCommand : CommandBase {
    public override string HelpText => "Utilities for StationeersLibrary";

    public override string[] Arguments => ["report"];

    public override bool IsLaunchCmd => false;

    public override string? Execute(string[] args) {
        /*if (args.First() == "report") {
            string logName = "LogForGithub";

            CommandLine.CommandsMap["log"].Execute(["logname", logName]);

            string title = HttpUtility.UrlEncode($"[LOG]: Log Uploaded, {DateTime.Now:d}");
            string body = HttpUtility.UrlEncode($"Please open `{Constants.STATIONEERS_DOCUMENTS_FOLDER}`, and drag and drop `{logName}.log` into this box. (you can delete this message)");
            string labels = HttpUtility.UrlEncode("log");

            Application.OpenURL($"{Constants.REPOSITORY_ISSUES_NEW_URL}?title={title}&body={body}&labels={labels}");

            return "Opened Github issue page.";
        }*/

        return null;
    }
}