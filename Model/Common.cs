using System.Globalization;

namespace InteligentnyDomWebViewer.Model
{
	public class Common
	{
		public static void SetDateFormat()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("pl-PL");
			Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortTimePattern = "HH:mm";
			Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongTimePattern = "HH:mm:ss";
			Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
			Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongDatePattern = "d MMMM yyyy";
		}
	}
}
