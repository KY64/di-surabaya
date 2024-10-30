import nunjucks from "nunjucks";
import fs from "node:fs";

const { BUS_MAP_FILE_ID, TRAIN_MAP_FILE_ID } = process.env;

const data = JSON.parse(fs.readFileSync("data.json"));

const values = {
  BUS_MAP_FILE_ID,
  TRAIN_MAP_FILE_ID,
  data
};

const result = nunjucks.render("index.njk", values);
fs.writeFileSync("index.html", result);
