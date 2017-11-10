namespace CHABS.Models {
	public class DropboxViewModel {
		public string UID { get; set; }
		public string DisplayName { get; set; }
		public string Email { get; set; }

		public bool AlreadyConnected { get; set; }
	}

	public class DropboxSettingsViewModel {
		public string FilePath { get; set; }
		public string FileName { get; set; }
	}
}