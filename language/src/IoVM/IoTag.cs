
using System.IO;


namespace io {

	public delegate IoObject IoTagCloneFunc(IoState state);
	public delegate void	IoTagFreeFunc();
	public delegate void	IoTagMarkFunc();
	public delegate void	IoTagNotificationFunc(object notification);
	public delegate void	IoTagCleanupFunc();
	public delegate IoObject	IoTagPerformFunc(IoObject target, IoObject locals, IoMessage message);
    public delegate IoObject IoTagActivateFunc(IoObject self, IoObject target, IoObject locals, IoMessage m, IoObject slotContext);
	public delegate int		IoTagCompareFunc(IoObject self, IoObject compareObject);
	public delegate void	IoTagWriteToStreamFunc(Stream stream);
	public delegate object	IoTagAllocFromStreamFunc(Stream stream);
	public delegate void	IoTagReadFromStreamFunc(Stream stream);

	public class IoTag {

		public IoState state;
		public string name;

		// memory management
		public IoTagCloneFunc cloneFunc;
		public IoTagCleanupFunc tagCleanupFunc;
		public IoTagNotificationFunc notificationFunc;

		// actions
		public IoTagPerformFunc performFunc; // lookup and activate, return result
		public IoTagActivateFunc activateFunc; // return the receiver or compute and return a value
		public IoTagCompareFunc compareFunc;

		// persistence
		public IoTagWriteToStreamFunc writeToStreamFunc;
		public IoTagAllocFromStreamFunc allocFromStreamFunc;
		public IoTagReadFromStreamFunc readFromStreamFunc;

		public static IoTag newWithName(string name) {
			IoTag tag = new IoTag();
			tag.name = name;
			return tag;
		}

	
	}	

}
