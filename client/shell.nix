# shell.nix
with import <nixpkgs> {};

mkShell {
  name = "dotnet-env";
  packages = [
    dotnet-sdk_8
    dotnet-runtime_8
  ];
}
