{
  "name": "{{name}}",
  "version": "0.1.0",
  "description": "Library {{name}}.",
  "main": "dist",
  "devDependencies": {
    "@annium/env-tsconfig-react": "0.1.0",
    "@annium/env-tslint-react": "0.1.0",
    "@types/node": "11.11.6",
    "@types/react": "16.8.8",
    "@types/react-dom": "16.8.3",
    "tslint": "5.14.0",
    "typescript": "3.3.4000",
    "typescript-tslint-plugin": "0.3.1"
  },
  "scripts": {
    "build": "tslint -p . && tsc -b",
    "lint": "tslint -p ."
  }
}
