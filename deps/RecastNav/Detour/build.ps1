# Script updates libDetour.lib
CC="zig cc" CXX="zig c++" cmake -B build;
cd build && make -j8;
