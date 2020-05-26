import { serviceConstants } from '../_constants';
import { serviceService } from '../_services';

export const serviceActions = {
    getAll,
    getByCode,
    update
};

function getAll() {
    return dispatch => {
        dispatch(request());

        serviceService.getAll()
            .then(
                services => dispatch(success(services)),
                error => dispatch(failure(error && error.message))
            );
    };

    function request() { return { type: serviceConstants.GETALL_REQUEST } }
    function success(services) { return { type: serviceConstants.GETALL_SUCCESS, services } }
    function failure(error) { return { type: serviceConstants.GETALL_FAILURE, error } }
}

function getByCode(code) {
    return dispatch => {
        dispatch(request());

        serviceService.getByCode(code)
            .then(
                service => dispatch(success(service)),
                error => dispatch(failure(error && error.message))
            );
    };

    function request() { return { type: serviceConstants.GETBYCODE_REQUEST } }
    function success(service) { return { type: serviceConstants.GETBYCODE_SUCCESS, service } }
    function failure(error) { return { type: serviceConstants.GETBYCODE_FAILURE, error } }
}

function update(serviceCode, data) {
    return dispatch => {
        dispatch(request());

        serviceService.update(serviceCode, data)
            .then(
                service => dispatch(success(service)),
                error => dispatch(failure(error && error.message))
            )
    };

    function request() { return { type: serviceConstants.UPDATE_REQUEST } }
    function success(service) { return { type: serviceConstants.UPDATE_SUCCESS, service } }
    function failure(error) { return { type: serviceConstants.UPDATE_FAILURE, error } }
}