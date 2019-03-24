/// <binding AfterBuild='Compile' />
/*
This file is the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. https://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp');
var exec = require('child_process').exec;

gulp.task('Compile', function (cb) {
    exec('ScriptBuilder SEScripts', { cwd: '../' }, function (err, stdout, stderr) {
        console.log(stdout);
        console.log(stderr);
        cb(err);
    });
});
