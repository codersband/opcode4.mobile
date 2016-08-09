/// <binding AfterBuild='nuget-pack' />

var gulp = require("gulp"),
    nuget = require("gulp-nuget");


var path = {
    nugetPath: 'D:\\Program Files\\nuget\\nuget.exe',
    nugetOutPath: 'D:\\Repositories\\Nuget'
}

gulp.task('nuget-pack', function () {
    return gulp.src('opcode4.mobile.csproj')
        .pipe(nuget.pack({
            nuget: path.nugetPath,
            outputDirectory: path.nugetOutPath, //'./nupkgs/',
            //version: '1.0.0',
            //basePath: './',
            exclude: 'gulpfile.js',//'**/*.designer.cs',
            //properties: 'configuration=release',
            //minClientVersion: '2.5',
            //msBuildVersion: '12',
            //build: true,
            //symbols: true,
            //excludeEmptyDirectories: true,
            includeReferencedProjects: true,
            //noDefaultExcludes: true,
            //tool: true
        }));
});