export class Response<T = void> {
  public data: T
  public error?: string
  get isSuccess() {
    return this.error === undefined
  }
  get isFailure() {
    return this.error !== undefined
  }

  constructor(data: T, error?: string) {
    this.data = data
    this.error = error
  }
}
