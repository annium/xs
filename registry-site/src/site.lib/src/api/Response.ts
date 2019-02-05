export default class Response<T = void> {
  public data: T
  public error: string | null
  get isSuccess() {
    return this.error === null
  }
  get isFailure() {
    return this.error !== null
  }

  constructor(data: T, error: string | null) {
    this.data = data
    this.error = error
  }
}
