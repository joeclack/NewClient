using NewClient.Interfaces;

namespace NewClient.Models
{
	public class Fan : IDevice
	{
		public int Id { get; set; }
		public bool IsOn { get; set; }
	}
}
