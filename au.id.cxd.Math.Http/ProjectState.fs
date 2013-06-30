namespace au.id.cxd.Math.Http

open System

open au.id.cxd.Math.Http.Filesystem
open au.id.cxd.Math.Http.Cache
open au.id.cxd.Math.Http.Project
open au.id.cxd.Math.Application

(** application module **)

module ProjectState = 

    let cacheName = "current_project"
    
    let projectFilename = "project.ser"
    
    let cacheProjectList = "project_list"

    (* internal *)
    /// save project to filesystem
    let saveToFilesystem name project = 
        Filesystem.saveToFilesystem name projectFilename project Project.writeRecord
    
    (* internal *)
    let storeInCache name project = 
        Cache.store cacheName project
        project
    
    (* internal *)
    let readFromCache name =
        match Cache.read name with
        | null -> None
        | item -> Some (item.Value :?> ProjectRecordState) 
    
    (* external *)
    let currentProject () = readFromCache cacheName
    
    (* external *)
    /// create a new project
    let create name = 
        let project = new ProjectRecordState()
        project.Application.ProjectName <- name
        saveToFilesystem name project.Project
        storeInCache cacheName project
        
    let saveCurrentProject () =
        let project = currentProject()
        match project with
        | None -> None
        | Some item ->
            saveToFilesystem item.Application.ProjectName item.Project
            Some item
    
    (* internal *)
    let enumerateDirectories () = 
        Filesystem.enumerateProjectDirectories ()
    
    (* internal *)
    let storeNamesInCache names = 
        Cache.store cacheProjectList names
        names
    
    (* external *)
    let retrieveNames () = 
        match Cache.read cacheProjectList with
        | null ->
            enumerateDirectories () 
            |> storeNamesInCache 
        | names -> names.Value :?> string []
            
    
    (* internal *)
    /// load a serialized project record from the filesystem
    let loadFromFilesystem name = 
        Filesystem.readFromFilesystem name projectFilename Project.readRecord
    
    (* external *)
    let load name = 
        let record = loadFromFilesystem name 
        let project = new ProjectRecordState()
        project.Project <- record
        storeInCache cacheName project 
        project

    
    ()