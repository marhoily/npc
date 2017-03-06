# Set of utilities that help to track INotifyPropertyChange, DependencyProperty, ObservableCollection, etc.
      
Helps to write expressions like: 
``` csharp
        myViewModel.Track(vm => vm.A.B.C.Name) 
```
and be sure
        to get notified whenewer `vm.A`, or `A.B`, or `C.Name` change.      
