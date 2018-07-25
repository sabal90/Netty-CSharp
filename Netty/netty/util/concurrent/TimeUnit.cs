using System;

namespace io.netty.util.concurrent
{
	public enum TimeUnit
	{
		NANOSECONDS,
		MICROSECONDS,
		MILLISECONDS,
		SECONDS,
		MINUTES,
		HOURS,
		DAYS
	};

	public static class TimeUnitFunction
	{
		static long C0 = 1L;
		static long C1 = C0 * 1000L;
		static long C2 = C1 * 1000L;
		static long C3 = C2 * 1000L;
		static long C4 = C3 * 60L;
		static long C5 = C4 * 60L;
		static long C6 = C5 * 24L;
		static long MAX = long.MaxValue;

		static long x(long d, long m, long over)
		{
			if (d > over) return long.MaxValue;
			if (d < -over) return long.MinValue;

			return d * m;
		}

		public static long toNanos(this TimeUnit unit, long d)
		{
			if (unit == TimeUnit.NANOSECONDS)	{ return d; }
			if (unit == TimeUnit.MICROSECONDS)	{ return x(d, C1 / C0, MAX / (C1 / C0)); }
			if (unit == TimeUnit.MILLISECONDS)	{ return x(d, C2 / C0, MAX / (C2 / C0)); }
			if (unit == TimeUnit.SECONDS)		{ return x(d, C3 / C0, MAX / (C3 / C0)); }
			if (unit == TimeUnit.MINUTES)		{ return x(d, C4 / C0, MAX / (C4 / C0)); }
			if (unit == TimeUnit.HOURS)			{ return x(d, C5 / C0, MAX / (C5 / C0)); }
			if (unit == TimeUnit.DAYS)			{ return x(d, C6 / C0, MAX / (C6 / C0)); }
			throw new ArgumentException("TimeUnit is wrong");
		}

		public static long toMicros(this TimeUnit unit, long d)
		{
			if (unit == TimeUnit.NANOSECONDS)	{ return d / (C1 / C0); }
			if (unit == TimeUnit.MICROSECONDS)	{ return d; }
			if (unit == TimeUnit.MILLISECONDS)	{ return x(d, C2 / C1, MAX / (C2 / C1)); }
			if (unit == TimeUnit.SECONDS)		{ return x(d, C3 / C1, MAX / (C3 / C1)); }
			if (unit == TimeUnit.MINUTES)		{ return x(d, C4 / C1, MAX / (C4 / C1)); }
			if (unit == TimeUnit.HOURS)			{ return x(d, C5 / C1, MAX / (C5 / C1)); }
			if (unit == TimeUnit.DAYS)			{ return x(d, C6 / C1, MAX / (C6 / C1)); }
			throw new ArgumentException("TimeUnit is wrong");
		}

		public static long toMillis(this TimeUnit unit, long d)
		{
			if (unit == TimeUnit.NANOSECONDS)	{ return d / (C2 / C0); }
			if (unit == TimeUnit.MICROSECONDS)	{ return d / (C2 / C1); }
			if (unit == TimeUnit.MILLISECONDS)	{ return d; }
			if (unit == TimeUnit.SECONDS)		{ return x(d, C3 / C2, MAX / (C3 / C2)); }
			if (unit == TimeUnit.MINUTES)		{ return x(d, C4 / C2, MAX / (C4 / C2)); }
			if (unit == TimeUnit.HOURS)			{ return x(d, C5 / C2, MAX / (C5 / C2)); }
			if (unit == TimeUnit.DAYS)			{ return x(d, C6 / C2, MAX / (C6 / C2)); }
			throw new ArgumentException("TimeUnit is wrong");
		}

		public static long toSeconds(this TimeUnit unit, long d)
		{
			if (unit == TimeUnit.NANOSECONDS)	{ return d / (C3 / C0); }
			if (unit == TimeUnit.MICROSECONDS)	{ return d / (C3 / C1); }
			if (unit == TimeUnit.MILLISECONDS)	{ return d / (C3 / C2); }
			if (unit == TimeUnit.SECONDS)		{ return d; }
			if (unit == TimeUnit.MINUTES)		{ return x(d, C4 / C3, MAX / (C4 / C3)); }
			if (unit == TimeUnit.HOURS)			{ return x(d, C5 / C3, MAX / (C5 / C3)); }
			if (unit == TimeUnit.DAYS)			{ return x(d, C6 / C3, MAX / (C6 / C3)); }
			throw new ArgumentException("TimeUnit is wrong");
		}

		public static long toMinutes(this TimeUnit unit, long d)
		{
			if (unit == TimeUnit.NANOSECONDS)	{ return d / (C4 / C0); }
			if (unit == TimeUnit.MICROSECONDS)	{ return d / (C4 / C1); }
			if (unit == TimeUnit.MILLISECONDS)	{ return d / (C4 / C2); }
			if (unit == TimeUnit.SECONDS)		{ return d / (C4 / C3); }
			if (unit == TimeUnit.MINUTES)		{ return d; }
			if (unit == TimeUnit.HOURS)			{ return x(d, C5 / C4, MAX / (C5 / C4)); }
			if (unit == TimeUnit.DAYS)			{ return x(d, C6 / C4, MAX / (C6 / C4)); }
			throw new ArgumentException("TimeUnit is wrong");
		}

		public static long toHours(this TimeUnit unit, long d)
		{
			if (unit == TimeUnit.NANOSECONDS)	{ return d / (C5 / C0); }
			if (unit == TimeUnit.MICROSECONDS)	{ return d / (C5 / C1); }
			if (unit == TimeUnit.MILLISECONDS)	{ return d / (C5 / C2); }
			if (unit == TimeUnit.SECONDS)		{ return d / (C5 / C3); }
			if (unit == TimeUnit.MINUTES)		{ return d / (C5 / C4); }
			if (unit == TimeUnit.HOURS)			{ return d; }
			if (unit == TimeUnit.DAYS)			{ return x(d, C6 / C5, MAX / (C6 / C5)); }
			throw new ArgumentException("TimeUnit is wrong");
		}

		public static long toDays(this TimeUnit unit, long d)
		{
			if (unit == TimeUnit.NANOSECONDS)	{ return d / (C6 / C0); }
			if (unit == TimeUnit.MICROSECONDS)	{ return d / (C6 / C1); }
			if (unit == TimeUnit.MILLISECONDS)	{ return d / (C6 / C2); }
			if (unit == TimeUnit.SECONDS)		{ return d / (C6 / C3); }
			if (unit == TimeUnit.MINUTES)		{ return d / (C6 / C4); }
			if (unit == TimeUnit.HOURS)			{ return d / (C6 / C5); }
			if (unit == TimeUnit.DAYS)			{ return d; }
			throw new ArgumentException("TimeUnit is wrong");
		}

		public static long convert(this TimeUnit unit, long d, TimeUnit u)
		{
			if (unit == TimeUnit.NANOSECONDS)	{ return u.toNanos(d); }
			if (unit == TimeUnit.MICROSECONDS)	{ return u.toMicros(d); }
			if (unit == TimeUnit.MILLISECONDS)	{ return u.toMillis(d); }
			if (unit == TimeUnit.SECONDS)		{ return u.toSeconds(d); }
			if (unit == TimeUnit.MINUTES)		{ return u.toMinutes(d); }
			if (unit == TimeUnit.HOURS)			{ return u.toHours(d); }
			if (unit == TimeUnit.DAYS)			{ return u.toDays(d); }
			throw new ArgumentException("TimeUnit is wrong");
		}
	}
}
