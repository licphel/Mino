using Mino.Nio;

namespace Mino.Framework;

/// <summary>
///     Universal async resource loader based on url.
/// </summary>
public class Loader {
	public delegate void UrlProcessor(Identifier id, Url resourceUrl);

	private int _processedCount;
	private Dictionary<Predicate<Url>, UrlProcessor> _processors =
		new Dictionary<Predicate<Url>, UrlProcessor>();
	private Queue<Loader> _subordinateLoaders = new Queue<Loader>();
	private Queue<Action> _taskQueue = new Queue<Action>();
	private int _totalCount;
	private string _scope;
	
	public Loader(string scope) {
		_scope = scope;
	}

	/// <summary>
	///     The base url of the loader for relative paths.
	/// </summary>
	public Url BaseUrl { get; set; } = Url.Runtime;

	/// <summary>
	///     Called before the first item loading.
	/// </summary>
	public Action BeginTask { get; set; } = () => { };

	/// <summary>
	///     Called after the last item loading.
	/// </summary>
	public Action EndTask { get; set; } = () => { };

	/// <summary>
	///     Current progress in [0.0, 1.0].
	/// </summary>
	public double Progress { get; private set; }

	/// <summary>
	///     Whether the loading has done. Equivalent to Progress == 1.0.
	/// </summary>
	public virtual bool Done { get; private set; }

	/// <summary>
	///     Dequeues the next task.
	/// </summary>
	public virtual void Next() {
		// Init stage.
		if (_processedCount == 0) {
			BeginTask();
			foreach (Loader c in _subordinateLoaders) {
				c.BeginTask();
			}
		}
		// Final stage.
		if (_taskQueue.Count == 0) {
			if (_subordinateLoaders.Count == 0) {
				Done = true;
				Progress = 1.0;
				EndTask();
				foreach (Loader c in _subordinateLoaders) {
					c.EndTask();
				}
				return;
			}

			Loader subLoader = _subordinateLoaders.Peek();
			subLoader.Next();
			++_processedCount;
			Progress = (double) _processedCount / _totalCount;
			if (subLoader.Done) {
				_subordinateLoaders.Dequeue();
			}
		} else {
			_taskQueue.Dequeue().Invoke();
			++_processedCount;
			Progress = (double) _processedCount / _totalCount;
		}
	}

	/// <summary>
	///     Adds a processor to the loader.
	/// </summary>
	/// <param name="condition">The processor's appliance condition.</param>
	/// <param name="processor">The processor function delegate.</param>
	public void AddProcessor(Predicate<Url> condition, UrlProcessor processor) {
		_processors[condition] = processor;
	}

	/// <summary>
	///     Enqueues all tasks in a loader into this loader.
	/// </summary>
	/// <param name="loader">The src loader.</param>
	/// <exception cref="Error">If the loading has begun.</exception>
	public void Enqueue(Loader loader) {
		if (loader == null) {
			return;
		}
		if (_processedCount != 0 || loader._processedCount != 0) {
			throw new Error("cannot enqueue while loading");
		}
		_subordinateLoaders.Enqueue(loader);
		_totalCount += loader._totalCount;
	}

	/// <summary>
	///     Enqueues a task.
	/// </summary>
	/// <param name="task">The task.</param>
	/// <exception cref="Error">If the loading has begun.</exception>
	public void Enqueue(Action task) {
		if (_processedCount != 0) {
			throw new Error("cannot enqueue while loading");
		}
		_taskQueue.Enqueue(task);
		++_totalCount;
		Done = false;
	}

	/// <summary>
	///     Enqueues a url and lets the loader to designate a task.
	/// </summary>
	/// <param name="url">The resource url.</param>
	/// <exception cref="Error">If the loading has begun.</exception>
	public void Enqueue(in Url url) {
		if (_processedCount != 0) {
			throw new Error("cannot enqueue while loading");
		}
		Url resName = Url.GetRelativeName(BaseUrl, url);
		designateToProcessor(new Identifier(_scope, resName.Path), url);
	}

	/// <summary>
	///     Scan through the base url.
	/// </summary>
	public void Scan() {
		Scan(BaseUrl);
	}

	/// <summary>
	///     Scan through the base url.
	/// </summary>
	/// <param name="baseUrl">Scan url root.</param>
	/// <param name="chRoot">Whether to set base path.</param>
	public void Scan(in Url baseUrl, bool chRoot = true) {
		if (chRoot) {
			BaseUrl = baseUrl;
		}
		
		FileUtil.PathType type = FileUtil.GetTypeOf(baseUrl);
		if (type == FileUtil.PathType.NotExist) {
			return;
		}
		if (type == FileUtil.PathType.File) {
			Enqueue(baseUrl);
			return;
		}
		// Scan recursively.
		IEnumerable<Url> subs = FileUtil.SubDirectories(baseUrl);
		foreach (Url dir in subs) {
			Scan(dir, false);
		}
		// Load files.
		subs = FileUtil.SubFiles(baseUrl);
		foreach (Url file in subs) {
			Enqueue(file);
		}
	}

	private void designateToProcessor(Identifier id, Url resourceUrl) {
		Enqueue(() => {
			foreach (KeyValuePair<Predicate<Url>, UrlProcessor> kv in _processors) {
				if (kv.Key(resourceUrl)) {
					kv.Value(id, resourceUrl);
				}
			}
		});
	}
}
