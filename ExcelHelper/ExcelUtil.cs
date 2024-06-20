using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelHelper
{
	internal class ExcelUtil : IDisposable
	{
		private Excel.Application? _excelApp;
        private Excel.Workbook? _workbook;
        private Excel.Worksheet? _worksheet;

        public ExcelUtil(string path, string sheetName) 
        {
	        try
	        {
				_excelApp = (Excel.Application)CustomMarshal.GetActiveObject("Excel.Application");
	        }
	        catch
	        {
				_excelApp = new Excel.Application
				{
					Visible = true
				};
	        }
            _workbook = _excelApp.Workbooks.Open(path);
            _worksheet = _workbook.Worksheets[sheetName];
        }

        public void ActivateSheet()
        {
	        _workbook?.Activate();
            _worksheet?.Activate();
        }

        public void SelectCell(int row, int column) 
        {
	        _workbook?.Activate();
            _worksheet?.Activate();
            _worksheet?.Cells[row + 1, column + 1].Select();
        }

        private static void ReleaseExcelObject(object? obj) 
        {
            try 
            {
                if (obj is not null) 
                {
                    Marshal.ReleaseComObject(obj);
                }
            }
            finally 
            {
                GC.Collect();
            }
        }

        public void Dispose()
        {
	        CleanUp();
        }

        private void CleanUp() 
        {
            ReleaseExcelObject(_worksheet);
            _worksheet = null;
            ReleaseExcelObject(_workbook);
            _workbook = null;
            ReleaseExcelObject(_excelApp);
            _excelApp = null;
        }
	}

	public static class CustomMarshal
	{
		internal const string OLEAUT32 = "oleaut32.dll";
		internal const string OLE32 = "ole32.dll";

		[SecurityCritical]
		public static object GetActiveObject( string progID ) 
		{
			object obj = null;
			Guid clsid;
			
			try 
			{
				CLSIDFromProgIDEx(progID, out clsid);
			}
			catch (Exception) 
			{
				CLSIDFromProgID(progID, out clsid);
			}

			GetActiveObject(ref clsid, IntPtr.Zero, out obj);
			return obj;
		}
		
		[DllImport(OLE32, PreserveSig = false)]
		[ResourceExposure(ResourceScope.None)]
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		private static extern void CLSIDFromProgIDEx( [MarshalAs(UnmanagedType.LPWStr)] String progId, out Guid clsid );
		
		[DllImport(OLE32, PreserveSig = false)]
		[ResourceExposure(ResourceScope.None)]
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		private static extern void CLSIDFromProgID( [MarshalAs(UnmanagedType.LPWStr)] String progId, out Guid clsid );
		
		[DllImport(OLEAUT32, PreserveSig = false)]
		[ResourceExposure(ResourceScope.None)]
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		private static extern void GetActiveObject( ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out Object ppunk );

	}
}
