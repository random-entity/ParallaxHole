void setup() {
  size(512, 512);
  
  for(int x = 0; x < width; x++) {
    for(int y = 0; y < height; y++) {
      if(x < 10 || x > width - 10 || y < 10 || y > height - 10) {
        stroke(0, 255, 0);
      } else if((x / 64 + y / 64) % 2 == 0) {
        stroke(0);
      } else {
        stroke(255);
      }
      point(x, y);
    }
  }
  
  save("checkerboard.png");
}
