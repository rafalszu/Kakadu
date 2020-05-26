import { captureConstants } from '../_constants';
import { captureService } from '../_services';

export const captureActions = {
    start,
    stop
};

function start(serviceCode) {
    return dispatch => {
        dispatch(request(serviceCode));

        captureService.start(serviceCode)
            .then(
                reponse => dispatch(success(serviceCode)),
                error => dispatch(failure(error))
            );

    };

    function request(serviceCode) { return { type: captureConstants.START_REQUEST, serviceCode: serviceCode } }
    function success(serviceCode) { return { type: captureConstants.START_SUCCESS, serviceCode: serviceCode } }
    function failure(error) { return { type: captureConstants.START_FAILURE, error } }
}

function stop(serviceCode) {
    return dispatch => {
        dispatch(request(serviceCode));

        captureService.stop(serviceCode)
            .then(
                Response => dispatch(success(serviceCode)),
                error => dispatch(failure(error))
            );
    };

    function request(serviceCode) { return { type: captureConstants.STOP_REQUEST, serviceCode: serviceCode } }
    function success(serviceCode) { return { type: captureConstants.STOP_SUCCESS, serviceCode: serviceCode } }
    function failure(error) { return { type: captureConstants.STOP_FAILURE, error } }
}
