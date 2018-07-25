
namespace io.netty.handler.timeout
{
	public class IdleStateEvent
	{
		public static IdleStateEvent FIRST_READER_IDLE_STATE_EVENT = new IdleStateEvent(IdleState.READER_IDLE, true);
		public static IdleStateEvent READER_IDLE_STATE_EVENT = new IdleStateEvent(IdleState.READER_IDLE, false);
		public static IdleStateEvent FIRST_WRITER_IDLE_STATE_EVENT = new IdleStateEvent(IdleState.WRITER_IDLE, true);
		public static IdleStateEvent WRITER_IDLE_STATE_EVENT = new IdleStateEvent(IdleState.WRITER_IDLE, false);
		public static IdleStateEvent FIRST_ALL_IDLE_STATE_EVENT = new IdleStateEvent(IdleState.ALL_IDLE, true);
		public static IdleStateEvent ALL_IDLE_STATE_EVENT = new IdleStateEvent(IdleState.ALL_IDLE, false);

		private IdleState _state;
		private bool first;

		protected IdleStateEvent(IdleState _state, bool first)
		{
			this._state = _state;
			this.first = first;
		}

		public IdleState state()
		{
			return _state;
		}

		public bool isFirst()
		{
			return first;
		}
	}
}
