export class ToastNotification {
    title: string;
    message: string;
    messageSmall: string;
    cssClass: string;

    static retryDelay: number = 50;
    static retryCount: number = 6;
    static titleDefault: string = 'POI Service';
    static cssClassSuccess: string = 'bi bi-check-lg text-primary';
    static cssClassError: string = 'bi bi-x-lg text-danger';

    constructor(title: string, message: string, messageSmall: string, cssClass: string) {
        this.title = title;
        this.message = message;
        this.messageSmall = messageSmall;
        this.cssClass = cssClass;
    }
}