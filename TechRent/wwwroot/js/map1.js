let map;
let deliveryMark;
let supplierOfficePlacemark = null;
let supplierOffice = null;
let selectedDeliveryPoint = null;

// Тариф доставки: рублей за км
const DELIVERY_RATE_PER_KM = 2;

function initMap(supplierLat, supplierLng, supplierName, supplierAddress) {
    // Преобразуем значения в числа и проверяем их валидность
    var lat = parseFloat(supplierLat);
    var lng = parseFloat(supplierLng);
    var name = String(supplierName || '').trim();
    var address = String(supplierAddress || '').trim();

    // Если координаты некорректные, используем значения по умолчанию (Минск)
    var isValidCoords = !isNaN(lat) && !isNaN(lng) && lat !== 0 && lng !== 0;

    if (typeof ymaps === 'undefined') {
        setTimeout(function () {
            initMap(supplierLat, supplierLng, supplierName, supplierAddress);
        }, 500);
        return;
    }

    ymaps.ready(function () {
        // Сохраняем данные офиса поставщика
        if (isValidCoords) {
            supplierOffice = {
                latitude: lat,
                longitude: lng,
                name: name || 'Офис поставщика',
                address: address || ''
            };
        }

        // Центр карты - офис поставщика или Минск по умолчанию
        var centerLat = supplierOffice && isValidCoords ? supplierOffice.latitude : 53.9045;
        var centerLng = supplierOffice && isValidCoords ? supplierOffice.longitude : 27.5615;
        var zoom = (supplierOffice && isValidCoords) ? 12 : 11;

        map = new ymaps.Map("map", {
            center: [centerLat, centerLng],
            zoom: zoom,
            controls: ['zoomControl', 'fullscreenControl']
        });

        addSearchControl();

        // Добавляем метку офиса поставщика (если есть)
        if (supplierOffice && isValidCoords) {
            supplierOfficePlacemark = new ymaps.Placemark(
                [supplierOffice.latitude, supplierOffice.longitude],
                {
                    balloonContent: '<strong>' + escapeHtml(supplierOffice.name) + '</strong>' + (supplierOffice.address ? '<br>' + escapeHtml(supplierOffice.address) : ''),
                    hintContent: escapeHtml(supplierOffice.name)
                },
                {
                    preset: 'islands#blueHomeCircleIcon'
                }
            );
            map.geoObjects.add(supplierOfficePlacemark);
        } else {
            // Если нет офиса поставщика, показываем сообщение
            var messagePlacemark = new ymaps.Placemark(
                [centerLat, centerLng],
                {
                    balloonContent: '<strong>Информация</strong><br>У поставщика этого оборудования еще не добавлен офис.<br>Выберите адрес доставки на карте.',
                    hintContent: 'Офис не добавлен'
                },
                {
                    preset: 'islands#grayStretchyIcon',
                    draggable: false
                }
            );
            map.geoObjects.add(messagePlacemark);
            messagePlacemark.balloon.open();
        }

        map.events.add('click', function (e) {
            var coords = e.get('coords');
            setDeliveryPoint(coords[0], coords[1]);
        });
    });
}

// Функция для экранирования HTML
function escapeHtml(str) {
    if (!str) return '';
    return str
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function setDeliveryPoint(lat, lng) {
    if (deliveryMark) {
        map.geoObjects.remove(deliveryMark);
    }

    deliveryMark = new ymaps.Placemark([lat, lng], {
        balloonContent: 'Точка доставки'
    }, {
        preset: 'islands#redDotIcon',
        draggable: true
    });

    deliveryMark.events.add('dragend', function () {
        var coords = deliveryMark.geometry.getCoordinates();
        updateDeliveryInfo(coords[0], coords[1]);
    });

    map.geoObjects.add(deliveryMark);
    updateDeliveryInfo(lat, lng);
}

function updateDeliveryInfo(lat, lng) {
    selectedDeliveryPoint = { lat: lat, lng: lng };
    var latInput = document.getElementById('deliveryLat');
    var lngInput = document.getElementById('deliveryLng');
    if (latInput) latInput.value = lat;
    if (lngInput) lngInput.value = lng;

    // Получаем адрес
    getAddressByCoords(lat, lng);

    // Считаем расстояние до офиса поставщика и цену доставки
    if (supplierOffice) {
        var distKm = haversineKm(lat, lng, supplierOffice.latitude, supplierOffice.longitude);
        var deliveryCost = Math.round(distKm * DELIVERY_RATE_PER_KM);

        // Сохраняем в hidden поля для использования при расчёте
        var distInput = document.getElementById('deliveryDistanceKm');
        var costInput = document.getElementById('deliveryCost');
        if (distInput) distInput.value = distKm.toFixed(2);
        if (costInput) costInput.value = deliveryCost;

        // Показываем инфо о доставке
        var infoEl = document.getElementById('deliveryDistanceInfo');
        if (infoEl) {
            infoEl.innerHTML = `
                <span class="material-symbols-outlined" style="font-size:16px; color:#3b82f6;">warehouse</span>
                Офис поставщика: <strong>${escapeHtml(supplierOffice.name)}</strong> — ${distKm.toFixed(1)} км
                &nbsp;|&nbsp;
                <span class="material-symbols-outlined" style="font-size:16px; color:#10b981;">local_shipping</span>
                Доставка: <strong>${deliveryCost.toLocaleString()} бел. руб.</strong>
            `;
            infoEl.style.display = 'flex';
        }

        // Обновляем расчёт цены
        if (typeof window.updatePriceCalculation === 'function') {
            window.updatePriceCalculation();
        }
    }
}

// Расстояние по формуле Haversine (км)
function haversineKm(lat1, lng1, lat2, lng2) {
    var R = 6371;
    var dLat = (lat2 - lat1) * Math.PI / 180;
    var dLng = (lng2 - lng1) * Math.PI / 180;
    var a = Math.sin(dLat / 2) ** 2 +
        Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
        Math.sin(dLng / 2) ** 2;
    return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}

function findNearestOffice(lat, lng) {
    if (!supplierOffice) return null;
    return {
        office: supplierOffice,
        distanceKm: haversineKm(lat, lng, supplierOffice.latitude, supplierOffice.longitude)
    };
}

async function getAddressByCoords(lat, lng) {
    try {
        var response = await fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&accept-language=ru`);
        var data = await response.json();
        var address = data.display_name || lat.toFixed(5) + ', ' + lng.toFixed(5);
        var addrEl = document.getElementById('deliveryAddress');
        if (addrEl) {
            addrEl.innerHTML = escapeHtml(address);
            var parent = addrEl.closest('.delivery-address-info');
            if (parent) parent.style.display = 'block';
        }
    } catch (e) {
        console.error('Error getting address:', e);
        var addrEl = document.getElementById('deliveryAddress');
        if (addrEl) {
            addrEl.innerHTML = lat.toFixed(5) + ', ' + lng.toFixed(5);
            var parent = addrEl.closest('.delivery-address-info');
            if (parent) parent.style.display = 'block';
        }
    }
}

function addSearchControl() {
    var searchControl = new ymaps.control.SearchControl({
        options: {
            provider: 'yandex#search',
            noPlacemark: true,
            resultsPerPage: 5
        }
    });

    searchControl.events.add('resultselect', function (e) {
        var index = e.get('index');
        searchControl.getResult(index).then(function (res) {
            var coords = res.geometry.getCoordinates();
            setDeliveryPoint(coords[0], coords[1]);
            map.setCenter(coords, 15);
        });
    });

    map.controls.add(searchControl, { position: { top: 10, right: 10 } });
}

// Функция для обновления карты при изменении офиса (вызывается извне)
function updateSupplierOffice(lat, lng, name, address) {
    supplierOffice = {
        latitude: lat,
        longitude: lng,
        name: name || 'Офис поставщика',
        address: address || ''
    };

    // Удаляем старую метку
    if (supplierOfficePlacemark) {
        map.geoObjects.remove(supplierOfficePlacemark);
    }

    // Добавляем новую метку
    supplierOfficePlacemark = new ymaps.Placemark(
        [lat, lng],
        {
            balloonContent: '<strong>' + escapeHtml(supplierOffice.name) + '</strong>' + (supplierOffice.address ? '<br>' + escapeHtml(supplierOffice.address) : ''),
            hintContent: escapeHtml(supplierOffice.name)
        },
        {
            preset: 'islands#blueHomeCircleIcon'
        }
    );
    map.geoObjects.add(supplierOfficePlacemark);
    map.setCenter([lat, lng], 12);
}