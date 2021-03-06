import { serviceConstants } from '../_constants';
import { serviceService } from '../_services';
import { history } from '../_helpers';

export const serviceActions = {
    getAll,
    getByCode,
    update,
    create,
    remove
};

function getAll() {
    return dispatch => {
        dispatch(request());

        serviceService.getAll()
            .then(
                services => dispatch(success(services)),
                error => dispatch(failure(error && error.response))
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
                error => dispatch(failure(error && error.response))
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
                error => dispatch(failure(error && error.response))
            )
    };

    function request() { return { type: serviceConstants.UPDATE_REQUEST } }
    function success(service) { return { type: serviceConstants.UPDATE_SUCCESS, service } }
    function failure(error) { return { type: serviceConstants.UPDATE_FAILURE, error } }
}

function create(service) {
    return dispatch => {
        dispatch(request());

        serviceService.create(service)
            .then(
                service => {
                    dispatch(success(service))
                    history.push(`/services/edit/${service.code}`)
                },
                error => dispatch(failure(error && error.response))
            )
    };

    function request() { return { type: serviceConstants.CREATE_REQUEST } }
    function success(service) { return { type: serviceConstants.CREATE_SUCCESS, service } }
    function failure(error) { return { type: serviceConstants.CREATE_FAILURE, error } }
}

function remove(serviceCode) {
    return dispatch => {
        dispatch(request(serviceCode));

        serviceService.remove(serviceCode)
            .then(
                result => dispatch(success(serviceCode)),
                error => dispatch(failure(error.response))
            )
    };

    function request(code) { return { type: serviceConstants.REMOVE_REQUEST, serviceCode: code } }
    function success(code) { return { type: serviceConstants.REMOVE_SUCCESS, serviceCode: code } }
    function failure(error) { return { type: serviceConstants.REMOVE_FAILURE, error } }
}