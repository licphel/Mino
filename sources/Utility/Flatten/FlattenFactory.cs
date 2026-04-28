namespace Mino.Utility.Flatten;

/// <summary>
///     A factory that creates and manages flattened state objects.
///     Each factory corresponds to a specific block type with its own set of properties.
///     States are encoded as bit-packed integers for efficient storage and manipulation.
/// </summary>
public sealed class FlattenFactory {
	private readonly List<Name> _names = new List<Name>();
	private readonly HashSet<Name> _fastNameC = new HashSet<Name>();
	private readonly List<NameLayout> _layouts = new List<NameLayout>();
	private int _totalBits;
	private bool _use64Bit;
	private readonly Dictionary<ulong, int> _compressedToId = new Dictionary<ulong, int>();
	private readonly List<ulong> _idToCompressed = new List<ulong>();

	/// <summary>
	///     Gets the total number of valid state combinations.
	/// </summary>
	public int StateCount {
		get => _idToCompressed.Count;
	}

	/// <summary>
	///     Adds a state property to this factory.
	/// </summary>
	/// <param name="name">The property name with its legal values.</param>
	/// <returns>This factory instance for method chaining.</returns>
	public FlattenFactory With(Name name) {
		_names.Add(name);
		_fastNameC.Add(name);
		return this;
	}

	/// <summary>
	///     Builds the factory by computing bit layouts for all properties
	///     and generating all possible state combinations.
	///     Must be called before using any state operations.
	/// </summary>
	public void Build() {
		int shift = 0;
		foreach (Name name in _names) {
			int bits = (int) Math.Ceiling(Math.Log2(name.Palette.Count));
			_layouts.Add(new NameLayout(name, shift, bits));
			shift += bits;
		}
		_totalBits = shift;
		_use64Bit = _totalBits <= 64;

		List<List<(Name, object)>> combinations = cartesian();

		foreach (List<(Name, object)> combo in combinations) {
			ulong compressed = compress(combo);
			if (!_compressedToId.ContainsKey(compressed)) {
				int id = _idToCompressed.Count;
				_compressedToId[compressed] = id;
				_idToCompressed.Add(compressed);
			}
		}
	}

	/// <summary>
	///     Creates a new state with the specified property values.
	///     Unspecified properties are set to their default values.
	/// </summary>
	/// <param name="args">Property-value pairs to set.</param>
	/// <returns>A new flattener representing the combined state.</returns>
	public Flattener Create(params (Name name, object value)[] args) {
		ulong compressed = 0;

		foreach (NameLayout layout in _layouts) {
			int idx = layout.GetIndex(layout.Name.InitValue);
			compressed |= (ulong) idx << layout.Shift;
		}

		foreach ((Name name, object value) in args) {
			NameLayout layout = layoutOf(name);
			int idx = layout.GetIndex(value);
			compressed = compressed & ~layout.Mask | (ulong) idx << layout.Shift;
		}

		return new Flattener(compressed);
	}

	/// <summary>
	///     Returns a new state with a single property changed.
	///     This is an O(1) operation using bit manipulation, no dictionary lookups.
	/// </summary>
	/// <param name="current">The current state.</param>
	/// <param name="name">The property to change.</param>
	/// <param name="newValue">The new value for the property.</param>
	/// <returns>A new flattener with the updated property.</returns>
	public Flattener With(Flattener current, Name name, object newValue) {
		NameLayout layout = layoutOf(name);
		int newIdx = layout.GetIndex(newValue);

		ulong newCompressed = current.Compressed & ~layout.Mask | (ulong) newIdx << layout.Shift;
		return new Flattener(newCompressed);
	}

	/// <summary>
	///     Retrieves the value of a property from a state.
	/// </summary>
	/// <typeparam name="T">The expected type of the property value.</typeparam>
	/// <param name="flattener">The state to query.</param>
	/// <param name="name">The property to retrieve.</param>
	/// <returns>The property value cast to the specified type.</returns>
	public T GetValue<T>(Flattener flattener, Name name) {
		NameLayout layout = layoutOf(name);
		int idx = (int) ((flattener.Compressed & layout.Mask) >> layout.Shift);
		return (T) layout.GetValue(idx);
	}

	/// <summary>
	///     Gets the compact integer identifier for a state.
	///     This ID is suitable for persistent storage and network transmission.
	/// </summary>
	/// <param name="flattener">The state to get the ID for.</param>
	/// <returns>A unique integer ID representing this state.</returns>
	public int GetId(Flattener flattener) {
		return _compressedToId[flattener.Compressed];
	}

	/// <summary>
	///     Recovers a state from its compact integer identifier.
	/// </summary>
	/// <param name="id">The state ID previously returned by GetId.</param>
	/// <returns>The reconstructed state.</returns>
	public Flattener FromId(int id) {
		return new Flattener(_idToCompressed[id]);
	}

	private NameLayout layoutOf(Name name) {
		return _layouts.First(l => l.Name == name);
	}

	private ulong compress(List<(Name name, object value)> combo) {
		ulong compressed = 0;
		foreach ((Name name, object value) in combo) {
			NameLayout layout = layoutOf(name);
			int idx = layout.GetIndex(value);
			compressed |= (ulong) idx << layout.Shift;
		}
		return compressed;
	}

	private List<List<(Name, object)>> cartesian() {
		var result = new List<List<(Name, object)>>();
		var options = new List<List<(Name, object)>>();

		foreach (NameLayout layout in _layouts) {
			var list = new List<(Name, object)>();
			foreach (object val in layout.Name.Values) {
				list.Add((layout.Name, val));
			}
			options.Add(list);
		}

		if (options.Count == 0) {
			result.Add(new List<(Name, object)>());
			return result;
		}

		result = new List<List<(Name, object)>> { new List<(Name, object)>() };

		foreach (List<(Name, object)> option in options) {
			var newResult = new List<List<(Name, object)>>();
			foreach (List<(Name, object)> existing in result) {
				foreach ((Name, object) val in option) {
					var copy = new List<(Name, object)>(existing) { val };
					newResult.Add(copy);
				}
			}
			result = newResult;
		}

		return result;
	}

	private sealed class NameLayout {
		public readonly Name Name;
		public readonly int Shift;
		public readonly int Bits;
		public readonly ulong Mask;

		public NameLayout(Name name, int shift, int bits) {
			Name = name;
			Shift = shift;
			Bits = bits;
			Mask = (1UL << bits) - 1 << shift;
		}

		public int GetIndex(object value) {
			return Name.Palette.IdFor(value);
		}

		public object GetValue(int index) {
			return Name.Palette.FromId(index);
		}
	}
}
