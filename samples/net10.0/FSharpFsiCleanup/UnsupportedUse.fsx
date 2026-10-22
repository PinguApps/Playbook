open System
let close (resource: IAsyncDisposable) = task { use owned = resource
                                             return 23 }
