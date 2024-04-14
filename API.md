### Components:

Range (struct):
- float min - Minimum value of the range
- float max - Maximum value of the range
- float size - Size of the range (max - min)
- bool IsValid - Whether the range is valid (min <= max)
- static Range Invalid - An invalid range with min > max
- static Range ZeroOne - A range from 0 to 1
- Range(float x) - Constructs a range with the same min and max value
- Range(float start, float end) - Constructs a range with the given start and end values

DesignCurve:
- float NeighbourBlendingBand - Blending factor between curve segments
- List<Vector2> Points - List of control points defining the curve
- List<SegmentType> Segments - List of segment types between control points
- float this[float x] - Evaluates the curve at a given x value
- void AddPoint(float x) - Adds a new control point at x, using curve value
- void AddPoint(Vector2 p) - Adds a new control point at the given position
- void DeletePoint(int index) - Deletes the control point at the given index
- void Sanitize() - Ensures the curve has at least two points and correct segments

Vector2b (struct):
- bool x - Boolean value representing x component
- bool y - Boolean value representing y component
- static Vector2b False - (false, false) vector
- static Vector2b True - (true, true) vector
- Vector2b(bool x, bool y) - Constructs a new Vector2b
- Vector2b(Vector2Int original) - Constructs from a Vector2Int
- Vector2b(Vector3Int original) - Constructs from a Vector3Int
- Overloaded operators for logic operations (&, |, ^, !, +, -, *)
- bool Equals(Vector2b other) - Checks equality with another Vector2b
- bool Equals(Vector3Int other) - Checks equality with a Vector3Int
- Vector2Int ToVector2Int() - Converts to a Vector2Int

Vector3b (struct):
- bool x - Boolean value representing x component
- bool y - Boolean value representing y component  
- bool z - Boolean value representing z component
- static Vector3b False - (false, false, false) vector
- static Vector3b True - (true, true, true) vector
- Vector3b(bool x, bool y, bool z) - Constructs a new Vector3b
- Vector3b(Vector2Int original) - Constructs from a Vector2Int
- Vector3b(Vector3Int original) - Constructs from a Vector3Int
- Overloaded operators for logic operations (&, |, ^, !, +, -, *)
- bool Equals(Vector3b other) - Checks equality with another Vector3b 
- bool Equals(Vector3Int other) - Checks equality with a Vector3Int
- Vector3Int ToVector3Int() - Converts to a Vector3Int

CircularCache<TKey, TValue>:
// Basically Dictionary with limited Capacity; when pushed beyond capacity - the oldest item gets popped
- int Capacity - Maximum number of items in the cache
- TValue this[TKey key] - Gets or sets the value associated with the key
- void ReInitialize() - Reinitializes the cache with the current capacity
- void ReInitialize(int capacity) - Reinitializes with a new capacity 
- void Clear() - Clears all items from the cache
- void Push(TKey key, TValue item, Action<TValue> poppedCallback = null) - Adds an item, optionally calling a callback when an item is popped
- bool Has(TKey key) - Checks if the key exists in the cache
- TValue Get(TKey key) - Retrieves the value associated with the key
- string Dump() - Returns a string representation of the cache contents

Mapping<TKey, TValue>:
- TValue this[TKey key] - Gets or sets the value associated with the key
- TKey GetByValue(TValue x) - Gets the key associated with the given value
- void Add(TKey key, TValue x) - Adds a new key-value pair
- void Clear() - Removes all key-value pairs
- bool Remove(TKey key) - Removes the key-value pair with the specified key

Pool<T>:
- Pool(T prefab, Transform root = null, bool autospawnRoot = true) - Constructor, takes a prefab and optionally a root transform and whether to auto-create the root
- void Pump(int amount) - Pre-spawns the given amount of objects in the pool
- T Get() - Gets an available object from the pool
- void Release(T item) - Returns an object back to the pool

SizedList<T>:
- int Count - Number of items in the list
- int Capacity - Maximum capacity of the list  
- T this[int index, bool sortByRecent = true] - Gets the item at the given index, optionally sorted by most recent
- void ReInitialize(int capacity) - Reinitializes the list with a new capacity
- void Clear() - Removes all items from the list
- void Add(T item, Action<T> poppedCallback = null) - Adds an item, optionally calling a callback when an item is popped

Square (struct):
- Vector2 min - Minimum corner of the square  
- Vector2 max - Maximum corner of the square
- Vector2 center - Center position of the square
- Vector2 size - Size of the square
- Vector2 cornerSW - Position of the south-west corner
- Vector2 cornerNW - Position of the north-west corner 
- Vector2 cornerNE - Position of the north-east corner
- Vector2 cornerSE - Position of the south-east corner
- Range xRange - Range along the x-axis
- Range yRange - Range along the y-axis
- bool Contains(Vector2 point) - Checks if a point is inside the square
- bool Intersects(Square other) - Checks if this square intersects another
- Vector2 ClosestPoint(Vector2 point) - Gets the closest point on the square's perimeter to the given point
- Vector2 ClosestPoint(Square other) - Gets the closest point in this square to the other square  
- float Distance(Vector2 point) - Distance to the given point
- float Distance(Square other) - Distance to the other square

Swapchain<T>:
- T A - First item in the swapchain
- T B - Second item 
- T C - Third item
- T D - Fourth item
- T this[int index] - Gets the item at the given index, wrapping around
- Swapchain(int depth, Func<T> spawner) - Constructs a swapchain of the given depth using a spawner function
- Swapchain(T a, T b) - Constructs a swapchain with two items
- Swapchain(T a, T b, T c) - Three items
- Swapchain(T a, T b, T c, T d) - Four items 
- Swapchain(T[] items) - Constructs from an array of items
- Swapchain(IEnumerable<T> items) - Constructs from an enumerable of items
- void Tick() - Advances to the next item in the swapchain

BaseSharedConfig:
- Abstract base class for shared configuration objects

BaseSharedConfig<T> where T: BaseConfig
- T Config - The configuration data of type T

MonoSingular<T> where T: MonoSingular<T>
// Static class that manages a single instance of type T
- static T s_Instance - The singleton instance

Timer (struct)
- bool IsOngoing - Whether the timer is currently running
- bool IsDone - Whether the timer has completed
- bool IsStarted - Whether the timer has been started
- float Factor - Normalized factor from 0 to 1 based on elapsed time  
- float Elapsed - Time elapsed since the timer started
- static Timer Off - A static "off" timer instance
- Timer(float duration, bool independent = false) - Constructs a new timer
- static void ShiftAll(float amount) - Shifts all active timers by the given amount

Springy (static class):
- Coeffs (struct) - Contains spring coefficients k1, k2, k3 and critical timestep dtCritical
- Float (struct) - Spring simulation for a single float value
  - float p - Position value  
  - float v - Velocity value
  - Coeffs ks - Spring coefficients
  - Float(float ownFreq, float dampFactor, float response, float startX) - Constructor with frequency, damping, response and start position
  - Float(Coeffs ks, float startX) - Constructor with coefficients and start position
  - float Tick(float x, float dt) - Simulate spring towards target x over timestep dt
  - float Tick(float x, float dx, float dt) - Simulate spring with velocity dx towards target x over timestep dt
- Vector2 (struct) - Spring simulation for a Vector2 value (same methods as Float)
- Vector3 (struct) - Spring simulation for a Vector3 value (same methods as Float)
- float Springy(float x, float dx, ref float y, ...) - Core spring simulation method for float  
- float2 Springy(float2 x, float2 dx, ref float2 y, ...) - Core for Vector2
- float3 Springy(float3 x, float3 dx, ref float3 y, ...) - Core for Vector3

ClickDetector:
- bool _isDoubleClick - Whether the last click was a double click
- float _doubleClickTime - Max time between clicks for double click
- float _lastClickTime - Time of the last click
- Action<ClickDetector> a_clicked - Callback for single click
- Action<ClickDetector> a_doubleClicked - Callback for double click  
- void OnPointerClick(PointerEventData) - Handles pointer click events

CloudItem:
- Vector2 Size - Size of the cloud item UI element
- bool IsDragged - Whether the item is currently being dragged

CloudView:
- void LateUpdate() - Runs a relaxation iteration on cloud item positions
- void _RelaxIteration(...) - Performs a single relaxation iteration

Draggable:
- bool IsDragged - Whether the object is currently being dragged
- void OnBeginDrag(PointerEventData) - Handles drag begin event
- void OnEndDrag(PointerEventData) - Handles drag end event 
- void OnDrag(PointerEventData) - Handles drag event

RectSelector:
- bool Active - Whether the selector is active and catching events
- Action<Rect> a_selection - Callback when a selection rect is made
- void OnBeginDrag(PointerEventData) - Overridden, starts rect selection
- void OnDrag(PointerEventData) - Overridden, updates selection rect  
- void OnEndDrag(PointerEventData) - Overridden, finishes selection

RasterCurve:
// AnimationCurve "rasterized" into an array for super-quick eval
- AnimationCurve RawCurve - The raw animation curve data
- float Evaluate(float inValue) - Evaluates the curve at the given input value

SceneTimer:
- void Start() - Starts the scene timer countdown
- void Update() - Updates and loads next scene when timer is up  

TimeOut:
- bool IsAvailable - Whether the timeout has elapsed
- void Start(float duration, TOBehaviour) - Starts timeout with duration and behaviour
- bool GetAtTime(float time) - Checks if timeout has elapsed at given time

TwoKeyDictionary<TKey1, TKey2, TValue>:
- TValue this[TKey1, TKey2] - Gets/sets value for the key pair
- IEnumerable<(TKey1, TKey2)> AllKeys - All key pairs in the dictionary
- IEnumerable<TValue> AllValues - All values in the dictionary
- TValue GetAdd(TKey1, TKey2, TValue template) - Gets value, adds if missing
- void Add(TKey1, TKey2, TValue) -  Adds a new key-value pair
- bool Remove(TKey1, TKey2) - Removes the key-value pair
- bool ContainsKeys(TKey1, TKey2) - Checks if key pair exists
- bool ContainsValue(TValue) - Checks if value exists
- bool TryGetValue(TKey1, TKey2, out TValue) - Tries to get value
- void ClearValues() - Clears all values in the dictionary

ValueCurve:
// Piece-wise linear curve defined by points
- float Evaluate(float timePoint) - Evaluates the curve value at a time point
