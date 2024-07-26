# Web

This folder contains the source code of di-surabaya website. It's built using an HTML templating engine.

## Requirements

- [NodeJS](https://nodejs.org/en/download/prebuilt-installer)

## How To Use

This part is step that we need to do after cloning/downloading this repository.

### 1. Install dependency

- Go to **Web** folder first, in a terminal we can do

```sh
cd src/Web;
```

- Install dependency

```sh
npm i
```

### 2. Set environment variable

There are 2 environment variables that need to be set

- `BUS_MAP_FILE_ID`: This is the ID of the bus map file in the Google Drive
- `TRAIN_MAP_FILE_ID`: This is the ID of the train map file in the Google Drive

In a terminal, we can set like this for example,

```
export BUS_MAP_FILE_ID=bus-example-id
export TRAIN_MAP_FILE_ID=train-example-id
```

### 3. Build the website

In the repository there is no HTML file but there is **index.njk** which is a template HTML
which uses [Nunjucks](https://mozilla.github.io/nunjucks/) syntax. Building the website
can be done this way,

```
node render.js
```

Once it's done, we will see **index.html** as the output.

## Rationale

di-surabaya is a plain and simple website. The goal of the website is to organize content
with simple representation. This goal can be achieved with just HTML and CSS. However,
some part of the content requires frequent update like the bus and train map. Since the
map file change is beyond control of the website owner, so the owner needs to follow
up the change by updating the map URL.

The change can be done by updating the HTML file, but then it requires a new commit everytime
there is a change. Although the change is simple, when it is done on mobile it becomes a hassle.
The solution is by making the URL stored in an environment variable. Since environment variable
can not be accessed through javascript in a browser, NodeJS is used to achieve this goal.

The NodeJS is used to access the environment variable and inject it to the HTML. Since injecting
value to specific element of HTML is challenging, an HTML template is used to solve this problem.
[Nunjucks](https://mozilla.github.io/nunjucks/) is chosen merely based on convenience.
Other HTML template engine can also solve the same problem and there is no tendency on which one 
better rather which one is more comfortable for the owner.

With HTML template engine, it is required to rebuild the website if there is any change. This build
step is done by Github CI which will retrieve the map URL from environment variable and then 
run javascript to inject it to the HTML.
