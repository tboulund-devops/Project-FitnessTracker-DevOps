{
  description = "Dotnet dev shell";

  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";

  outputs =
    { self, nixpkgs }:
    let
      system = "x86_64-linux";
      pkgs = nixpkgs.legacyPackages.${system};
    in
    {
      devShells.${system}.default = pkgs.mkShell {
        packages = with pkgs; [
          dotnet-sdk_10

          # SkiaSharp/Avalonia native runtime deps
          fontconfig
          freetype
          harfbuzz
          icu
          libGL
          libX11
          libXcursor
          libXext
          libXfixes
          libXi
          libXrandr
          libXrender
          libxcb
          libxkbcommon
          libICE
          libSM
          openssl
          stdenv.cc.cc.lib
          zlib
        ];

        DOTNET_ROOT = pkgs.dotnet-sdk_10;

        # Help native deps resolve at runtime inside the dev shell.
        LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath (
          with pkgs;
          [
            fontconfig
            freetype
            harfbuzz
            icu
            libGL
            libX11
            libXcursor
            libXext
            libXfixes
            libXi
            libXrandr
            libXrender
            libxcb
            libxkbcommon
            libICE
            libSM
            openssl
            stdenv.cc.cc.lib
            zlib
          ]
        );
      };
    };
}
