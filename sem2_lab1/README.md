I split up the UI into 4-5 kinds of objects:
1. The raw data or services used in some context (ModelData). This object can be a value type and is not itself observable, it's just a bunch of properties or fields.
2. The object that wraps this data, making it observable and ensuring its consistency (Model).
3. The object containing the methods for interacting with the model (I just call it Service).
4. The object that is used in a particular kind of view, aka in a window or a user control, that provides all of the properties it needs. This object is set as the `DataContext` of the view class. This object is also reactive. (ViewModel).
5. The last object is the particular class that inherits from Window or UserControl (View).
  Usually, all it defines are adapters for event handlers, and binding the view model as the DataContext.
  It doesn't contains any logic itself, it must wire all calls back to the ViewModel or to the Service, unless they are to do directly with UI elements.
  It may have references to UI objects (the ViewModel is at a higher level of abstraction and must not interact with these).